using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.E2E.Tests;

/// <summary>End-to-end tests for the top-bar language selector.</summary>
[TestClass]
public sealed class LanguageSelectorTests : PlaywrightTestBase
{
    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task Shows_five_language_flags()
    {
        await Page.GotoAsync("/", Idle);
        await Expect(Page.Locator(".language-selector img")).ToHaveCountAsync(5);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("language-selector.png") });
    }

    [TestMethod]
    public async Task Selecting_a_language_persists_culture_and_reloads()
    {
        await Page.GotoAsync("/", Idle);

        await Page.GetByAltText("Svenska").ClickAsync();

        // Clicking stores the culture then force-reloads; after reload the flag is marked active.
        await Expect(Page.GetByAltText("Svenska")).ToHaveClassAsync(new Regex(@"(^|\s)active(\s|$)"));

        var stored = await Page.EvaluateAsync<string?>("() => localStorage.getItem('planning.ui.culture')");
        Assert.AreEqual("sv-SE", stored);
    }

    [TestMethod]
    public async Task About_button_text_is_localized()
    {
        await Page.GotoAsync("/", Idle);
        await Expect(Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "About" })).ToBeVisibleAsync();

        await Page.GetByAltText("Svenska").ClickAsync();

        // After switching to Swedish the resx label resolves: About -> Om.
        await Expect(Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Om" })).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("about-localized-sv.png") });
    }

    [TestMethod]
    public async Task About_dialog_loads_markdown_from_content()
    {
        await Page.GotoAsync("/", Idle);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "About" }).ClickAsync();

        // The dialog renders About.md fetched from the Translations RCL's _content path.
        await Expect(Page.Locator(".about .panel")).ToContainTextAsync("Features");
    }
}
