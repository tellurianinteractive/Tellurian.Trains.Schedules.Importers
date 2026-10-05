using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for a train category's stop pattern on the Train categories tab: the layout's
/// operating locations are offered to tick, ticking one gives the category a pattern, and clearing takes
/// it away again.
/// </summary>
[TestClass]
public sealed class StopPatternUiTests : PlaywrightTestBase
{
    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task A_stop_pattern_is_ticked_from_the_layouts_operating_locations()
    {
        var pageErrors = new List<string>();
        Page.PageError += (_, error) => pageErrors.Add(error);

        await Page.GotoAsync("/", Idle);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "New layout" }).ClickAsync();

        await OpenTabAsync("Operation locations");
        await AddStation("Göteborg", "G");
        await AddStation("Kungsbacka", "Kb");

        // In-app navigation, not a reload: the plan is written to the browser store on a debounce, so a
        // reload here could land before the two stations are in it.
        await OpenTabAsync("Train categories");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit" }).First.ClickAsync();

        // Both stations exchange passengers and cargo, so either category is offered both of them.
        var locations = Page.Locator(".stop-locations li");
        await Expect(locations).ToHaveCountAsync(2);
        await Expect(Page.Locator(".stop-pattern")).ToContainTextAsync("Kungsbacka");

        var clear = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear" });
        await Expect(clear).ToBeDisabledAsync();

        await locations.First.Locator("input").CheckAsync();
        await Expect(clear).ToBeEnabledAsync();
        await Page.Locator(".stop-pattern").ScrollIntoViewIfNeededAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("stop-pattern.png"), FullPage = true });

        await clear.ClickAsync();
        await Expect(locations.First.Locator("input")).Not.ToBeCheckedAsync();
        await Expect(clear).ToBeDisabledAsync();

        Assert.AreEqual(0, pageErrors.Count, "Uncaught JS errors: " + string.Join(" | ", pageErrors));
    }

    [TestMethod]
    public async Task A_shunting_category_is_offered_no_stop_pattern()
    {
        await Page.GotoAsync("/", Idle);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "New layout" }).ClickAsync();

        await OpenTabAsync("Operation locations");
        await AddStation("Göteborg", "G");

        await OpenTabAsync("Train categories");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add new" }).ClickAsync();
        await Field("Type").Locator("select").SelectOptionAsync(new SelectOptionValue { Label = "Shunting task" });

        // A task works at one place and travels nowhere, so there is no route for a pattern to shape.
        await Expect(Page.Locator(".stop-pattern")).ToHaveCountAsync(0);
    }

    private Task OpenTabAsync(string name) =>
        Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = name }).ClickAsync();

    private ILocator Field(string label) =>
        Page.Locator(".field", new PageLocatorOptions { HasTextString = label });

    private async Task AddStation(string name, string signature)
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add new" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Station", Exact = true }).ClickAsync();
        await Field("Name").Locator("input").First.FillAsync(name);
        await Field("Signature").Locator("input").FillAsync(signature);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).ClickAsync();
    }
}
