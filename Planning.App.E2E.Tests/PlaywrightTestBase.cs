using Microsoft.Playwright;

namespace Tellurian.Trains.Schedules.Planning.App.E2E.Tests;

/// <summary>
/// Base class giving each test an isolated <see cref="IBrowserContext"/> and <see cref="IPage"/>
/// over the shared browser, with the app's base URL pre-configured and a screenshot helper.
/// </summary>
public abstract class PlaywrightTestBase
{
    /// <summary>The per-test browser context (isolated cookies/localStorage).</summary>
    protected IBrowserContext Context { get; private set; } = default!;

    /// <summary>The per-test page.</summary>
    protected IPage Page { get; private set; } = default!;

    [TestInitialize]
    public async Task InitPageAsync()
    {
        Context = await AppFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = AppFixture.BaseUrl
        });
        Page = await Context.NewPageAsync();
    }

    [TestCleanup]
    public async Task DisposePageAsync() => await Context.DisposeAsync();

    /// <summary>Path under the test output's <c>screenshots</c> folder for a named PNG.</summary>
    protected static string ScreenshotPath(string fileName)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileName);
    }
}
