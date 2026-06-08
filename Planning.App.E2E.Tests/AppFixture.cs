using System.Diagnostics;
using Microsoft.Playwright;

namespace Tellurian.Trains.Schedules.Planning.App.E2E.Tests;

/// <summary>
/// Assembly-wide fixture: ensures the Planning.App is running and a headless browser is available.
/// Set the environment variable <c>PLANNING_APP_BASEURL</c> to point the tests at an already-running
/// server (e.g. one launched by the run-skill); otherwise the fixture launches it with
/// <c>dotnet run --launch-profile http</c> and stops it on cleanup.
/// </summary>
[TestClass]
public static class AppFixture
{
    private static Process? _appProcess;
    private static IPlaywright? _playwright;

    /// <summary>The base URL the tests navigate to.</summary>
    public static string BaseUrl { get; private set; } = "http://localhost:5097";

    /// <summary>The shared headless browser; each test creates its own isolated context.</summary>
    public static IBrowser Browser { get; private set; } = default!;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        var configured = Environment.GetEnvironmentVariable("PLANNING_APP_BASEURL");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            BaseUrl = configured.TrimEnd('/');
        }
        else
        {
            BaseUrl = "http://localhost:5097";
            _appProcess = StartApp();
        }

        await WaitForServerAsync(BaseUrl, TimeSpan.FromMinutes(2));

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (Browser is not null) await Browser.DisposeAsync();
        _playwright?.Dispose();
        if (_appProcess is { HasExited: false })
        {
            try { _appProcess.Kill(entireProcessTree: true); } catch { /* best effort */ }
        }
    }

    private static Process StartApp()
    {
        var appProject = Path.Combine(FindRepoRoot(), "Planning.App");
        var psi = new ProcessStartInfo("dotnet", $"run --project \"{appProject}\" --launch-profile http")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start Planning.App.");
        // Drain output so the child process does not block on a full pipe.
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static async Task WaitForServerAsync(string baseUrl, TimeSpan timeout)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await http.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode) return;
            }
            catch
            {
                // Server not accepting connections yet.
            }
            await Task.Delay(1000);
        }
        throw new TimeoutException($"Planning.App did not become ready at {baseUrl} within {timeout}.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !dir.EnumerateFiles("*.slnx").Any())
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Could not locate the repository root (no .slnx found).");
    }
}
