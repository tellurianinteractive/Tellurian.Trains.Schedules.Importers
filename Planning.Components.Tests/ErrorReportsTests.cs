using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Tellurian.Trains.Schedules.Planning.Components.Services;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that errors are kept for reporting: logged ones are caught, they survive a reload through
/// localStorage, and the report text carries the stack trace.
/// </summary>
[TestClass]
public class ErrorReportsTests
{
    [TestMethod]
    public void AnErrorLoggedWithAnExceptionIsKept()
    {
        var reports = new ErrorReports(new FakeBrowser());
        var logger = new ErrorReportLoggerProvider(reports).CreateLogger("Validation");

        logger.LogError(Thrown(), "Validation failed");

        Assert.HasCount(1, reports.Reports);
        Assert.AreEqual("Validation", reports.Reports[0].Source);
        Assert.AreEqual("Validation failed", reports.Reports[0].Message);
        Assert.Contains(nameof(Thrown), reports.Reports[0].Details);
    }

    [TestMethod]
    public void AnErrorWithoutExceptionOrAWarningIsNotKept()
    {
        var reports = new ErrorReports(new FakeBrowser());
        var logger = new ErrorReportLoggerProvider(reports).CreateLogger("Any");

        logger.LogError("No exception");
        logger.LogWarning(Thrown(), "Only a warning");

        Assert.IsFalse(reports.Any);
    }

    [TestMethod]
    public void ReportsSurviveAReload()
    {
        var browser = new FakeBrowser();
        new ErrorReports(browser).Add("Renderer", "Unhandled", Thrown());

        var afterReload = new ErrorReports(browser);

        Assert.HasCount(1, afterReload.Reports);
        Assert.AreEqual("Renderer", afterReload.Reports[0].Source);
    }

    [TestMethod]
    public void ClearingForgetsTheReportsAlsoAfterAReload()
    {
        var browser = new FakeBrowser();
        var reports = new ErrorReports(browser);
        reports.Add("Renderer", "Unhandled", Thrown());

        reports.Clear();

        Assert.IsFalse(reports.Any);
        Assert.IsFalse(new ErrorReports(browser).Any);
    }

    [TestMethod]
    public void OnlyTheNewestReportsAreKept()
    {
        var reports = new ErrorReports(new FakeBrowser());

        for (var i = 1; i <= 25; i++) reports.Add("Source", $"Error {i}", null);

        Assert.HasCount(20, reports.Reports);
        Assert.AreEqual("Error 25", reports.Reports[^1].Message);
    }

    [TestMethod]
    public void TheTextHasTheAppVersionAndTheStackTraceNewestFirst()
    {
        var reports = new ErrorReports(new FakeBrowser());
        reports.Add("First", "Older", null);
        reports.Add("Second", "Newer", Thrown());

        var text = reports.ToText("Timetable Planner", "1.5.2");

        Assert.StartsWith("Timetable Planner 1.5.2", text);
        Assert.IsLessThan(text.IndexOf("Older", StringComparison.Ordinal), text.IndexOf("Newer", StringComparison.Ordinal));
        Assert.Contains(nameof(Thrown), text);
    }

    [TestMethod]
    public void WithoutABrowserTheReportsAreKeptInMemory()
    {
        var reports = new ErrorReports(new FakeServerRuntime());

        reports.Add("Source", "Error", Thrown());

        Assert.HasCount(1, reports.Reports);
    }

    private static Exception Thrown()
    {
        try { throw new InvalidOperationException("Boom"); }
        catch (Exception ex) { return ex; }
    }

    /// <summary>Stands in for the browser's in-process JS runtime, with localStorage as a dictionary.</summary>
    private sealed class FakeBrowser : IJSInProcessRuntime
    {
        private readonly Dictionary<string, string> _storage = [];

        public TResult Invoke<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(string identifier, params object?[]? args)
        {
            var key = (string)args![0]!;
            switch (identifier)
            {
                case "localStorage.getItem":
                    return (TResult)(object?)_storage.GetValueOrDefault(key)!;
                case "localStorage.setItem":
                    _storage[key] = (string)args[1]!;
                    return default!;
                case "localStorage.removeItem":
                    _storage.Remove(key);
                    return default!;
                default:
                    throw new NotSupportedException(identifier);
            }
        }

        public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args) =>
            ValueTask.FromResult(Invoke<TValue>(identifier, args));

        public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            ValueTask.FromResult(Invoke<TValue>(identifier, args));
    }

    /// <summary>A JS runtime that is not in process, as under an interactive Server host.</summary>
    private sealed class FakeServerRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args) =>
            throw new NotSupportedException();

        public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            throw new NotSupportedException();
    }
}
