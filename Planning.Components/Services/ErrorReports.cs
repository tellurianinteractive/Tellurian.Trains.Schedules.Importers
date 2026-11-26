using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>One error the app ran into: when, where it was logged from, and the full exception text.</summary>
/// <param name="Time">When the error happened, in UTC.</param>
/// <param name="Source">The logger category or other source that reported it.</param>
/// <param name="Message">The logged message.</param>
/// <param name="Details">The exception with its stack trace, or empty when none was logged.</param>
public sealed record ErrorReport(DateTimeOffset Time, string Source, string Message, string Details);

/// <summary>
/// The errors this browser has run into, kept until the user clears them, so they can be reported. They are
/// kept in <c>localStorage</c> as well as in memory, because an unhandled exception usually ends with the user
/// reloading the app, and the report is wanted most exactly then.
/// </summary>
/// <remarks>
/// A singleton by necessity: logging providers are singletons and must be able to reach it. It holds no plan
/// data, only error texts, and is meant for the WebAssembly host, where there is one user per process and
/// <c>localStorage</c> can be reached synchronously (<see cref="IJSInProcessRuntime"/>). Elsewhere it keeps the
/// reports in memory only.
/// </remarks>
public sealed class ErrorReports
{
    private const string StorageKey = "tellurian.planner.errorReports";
    private const int MaxReports = 20;
    private const int MaxDetailsLength = 16_000;

    private readonly IJSInProcessRuntime? _js;
    private readonly Lock _lock = new();
    private List<ErrorReport> _reports = [];
    private bool _loaded;
    [ThreadStatic] private static bool _adding;

    public ErrorReports(IJSRuntime js) => _js = js as IJSInProcessRuntime;

    /// <summary>Raised after a report is added or the reports are cleared.</summary>
    public event Action? Changed;

    /// <summary>The kept reports, oldest first.</summary>
    public IReadOnlyList<ErrorReport> Reports
    {
        get
        {
            lock (_lock)
            {
                EnsureLoaded();
                return [.. _reports];
            }
        }
    }

    /// <summary>Whether there is anything to report.</summary>
    public bool Any => Reports.Count > 0;

    /// <summary>Keeps an error. Never throws: reporting an error must not cause another.</summary>
    /// <param name="source">Where the error was reported from, such as a logger category.</param>
    /// <param name="message">What was logged.</param>
    /// <param name="exception">The exception, whose text and stack trace are kept; may be null.</param>
    public void Add(string source, string message, Exception? exception)
    {
        if (_adding) return; // an error while keeping an error is not kept again
        _adding = true;
        try
        {
            var details = exception?.ToString() ?? string.Empty;
            if (details.Length > MaxDetailsLength) details = details[..MaxDetailsLength] + " …";
            lock (_lock)
            {
                EnsureLoaded();
                _reports.Add(new(DateTimeOffset.UtcNow, source, message, details));
                if (_reports.Count > MaxReports) _reports.RemoveRange(0, _reports.Count - MaxReports);
                Save();
            }
            Changed?.Invoke();
        }
        catch
        {
            // Nothing sensible is left to do.
        }
        finally
        {
            _adding = false;
        }
    }

    /// <summary>Forgets every kept report, for example after they have been sent.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            _reports = [];
            _loaded = true;
            try { _js?.InvokeVoid("localStorage.removeItem", StorageKey); } catch { /* memory is cleared anyway */ }
        }
        Changed?.Invoke();
    }

    /// <summary>
    /// The reports as plain text for a mail or the clipboard: the app and its version first, then each error with
    /// its time, source, message and stack trace, newest first.
    /// </summary>
    /// <param name="appName">The app's name.</param>
    /// <param name="appVersion">The app's version.</param>
    public string ToText(string appName, string appVersion)
    {
        var text = new StringBuilder();
        text.Append(appName).Append(' ').AppendLine(appVersion);
        foreach (var report in Reports.Reverse())
        {
            text.AppendLine().AppendLine("----");
            text.Append("Time: ").AppendLine(report.Time.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", System.Globalization.CultureInfo.InvariantCulture));
            text.Append("Source: ").AppendLine(report.Source);
            text.Append("Message: ").AppendLine(report.Message);
            if (report.Details.Length > 0) text.AppendLine(report.Details);
        }
        return text.ToString();
    }

    private void EnsureLoaded()
    {
        if (_loaded || _js is null) return;
        _loaded = true;
        try
        {
            var json = _js.Invoke<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
                _reports = [.. JsonSerializer.Deserialize(json, ErrorReportsJsonContext.Default.ListErrorReport) ?? [], .. _reports];
        }
        catch
        {
            // Unreadable reports from an older version are dropped.
        }
    }

    private void Save()
    {
        if (_js is null) return;
        try
        {
            _js.InvokeVoid("localStorage.setItem", StorageKey, JsonSerializer.Serialize(_reports, ErrorReportsJsonContext.Default.ListErrorReport));
        }
        catch
        {
            // Kept in memory still; storage may be full or unavailable.
        }
    }
}

/// <summary>Source-generated serialisation of the kept error reports.</summary>
[JsonSerializable(typeof(List<ErrorReport>))]
internal sealed partial class ErrorReportsJsonContext : JsonSerializerContext;

/// <summary>
/// Keeps every error logged with an exception, and every critical one, in <see cref="ErrorReports"/>. Blazor logs
/// unhandled exceptions in components this way, and so do the app's own guarded failures, such as a validation
/// pass that could not finish.
/// </summary>
public sealed class ErrorReportLoggerProvider(ErrorReports reports) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new ErrorReportLogger(categoryName, reports);

    public void Dispose() { }

    private sealed class ErrorReportLogger(string category, ErrorReports reports) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel < LogLevel.Error || (exception is null && logLevel < LogLevel.Critical)) return;
            reports.Add(category, formatter(state, exception), exception);
        }
    }
}

/// <summary>Registers error reporting.</summary>
public static class ErrorReportsServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ErrorReports"/> and the logging provider that fills it. For the WebAssembly host: see
    /// <see cref="ErrorReports"/>.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    public static IServiceCollection AddErrorReports(this IServiceCollection services)
    {
        services.AddSingleton<ErrorReports>();
        services.AddSingleton<ILoggerProvider, ErrorReportLoggerProvider>();
        return services;
    }
}
