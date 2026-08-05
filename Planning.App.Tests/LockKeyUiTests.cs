using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for the lock key on the Operation Locations tab: the field is offered where a key
/// can be needed, only manned stations are offered to hold it, and what is set survives the save.
/// </summary>
[TestClass]
public sealed class LockKeyUiTests : PlaywrightTestBase
{
    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task A_lock_key_can_be_given_to_an_industrial_area()
    {
        var pageErrors = new List<string>();
        Page.PageError += (_, error) => pageErrors.Add(error);

        await Page.GotoAsync("/", Idle);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "New layout" }).ClickAsync();
        await Page.GotoAsync("/operation-locations", Idle);

        // A manned station to hold the key. A station added here is manned from the start.
        await AddLocation("Station", "Göteborg", "G");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).ClickAsync();

        // The location the key unlocks: an industrial area is never manned and always works cargo.
        await AddLocation("Industrial area", "Bruket", "Bru");

        var heldAt = Field("Lock key held at").Locator("select");
        await Expect(heldAt).ToBeVisibleAsync();

        // The name is asked for only once a station holds the key: a name with nowhere to fetch it
        // from tells the driver nothing.
        await Expect(Field("Lock key name")).ToHaveCountAsync(0);
        await heldAt.SelectOptionAsync(new SelectOptionValue { Label = "Göteborg (G)" });

        var name = Field("Lock key name").Locator("input");
        await Expect(name).ToBeVisibleAsync();
        await name.FillAsync("A1");
        await name.BlurAsync();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).ClickAsync();

        // Back on the list, the key column names the key and where it is fetched.
        await Expect(Page.Locator("table")).ToContainTextAsync("A1 (G)");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("lockkey-list.png"), FullPage = true });

        Assert.AreEqual(0, pageErrors.Count, "Uncaught JS errors: " + string.Join(" | ", pageErrors));
    }

    [TestMethod]
    public async Task Manning_the_location_leaves_the_key_ignored_but_editable()
    {
        await Page.GotoAsync("/", Idle);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "New layout" }).ClickAsync();
        await Page.GotoAsync("/operation-locations", Idle);

        await AddLocation("Station", "Göteborg", "G");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).ClickAsync();

        // An unmanned station this time, so the manning can then be put back on.
        await AddLocation("Station", "Bruket", "Bru");
        await Field("Manned?").Locator("input").UncheckAsync();
        await Field("Lock key held at").Locator("select").SelectOptionAsync(new SelectOptionValue { Label = "Göteborg (G)" });
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit" }).Last.ClickAsync();
        await Field("Manned?").Locator("input").CheckAsync();

        // The key is kept and still on the form, so it can be corrected or cleared here — and it says
        // it is not in force.
        await Expect(Field("Lock key held at").Locator("select")).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"\d+"));
        await Expect(Page.Locator(".hint.ignored")).ToBeVisibleAsync();
        // The tab scrolls inside its own pane, so a full-page shot would stop at the fold.
        await Page.Locator(".hint.ignored").ScrollIntoViewIfNeededAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("lockkey-ignored.png"), FullPage = true });

        // And the plan reports it under Conflicts.
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Conflicts" }).ClickAsync();
        await Expect(Page.Locator(".conflict-list")).ToContainTextAsync("Bruket");
    }

    [TestMethod]
    public async Task A_manned_station_is_not_offered_a_lock_key()
    {
        await Page.GotoAsync("/", Idle);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "New layout" }).ClickAsync();
        await Page.GotoAsync("/operation-locations", Idle);

        await AddLocation("Station", "Göteborg", "G");

        // Somebody is on duty there to work the switches, so there is no key to fetch.
        await Expect(Field("Lock key held at")).ToHaveCountAsync(0);
    }

    private ILocator Field(string label) =>
        Page.Locator(".field", new PageLocatorOptions { HasTextString = label });

    private async Task AddLocation(string type, string name, string signature)
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add new" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = type, Exact = true }).ClickAsync();
        await Field("Name").Locator("input").First.FillAsync(name);
        await Field("Signature").Locator("input").FillAsync(signature);
    }
}
