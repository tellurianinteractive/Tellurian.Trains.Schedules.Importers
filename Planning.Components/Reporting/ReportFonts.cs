namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// The font families a report may be set in, and the CSS that applies the planner's choice.
/// </summary>
/// <remarks>
/// <para>
/// Only reports use this. The interface keeps its own font, so a choice made for print never changes
/// what the plan looks like while it is being worked on.
/// </para>
/// <para>
/// The catalogue is a list of families to <em>offer</em>, not a promise that any of them exist: which
/// ones are installed differs per machine, so <c>js/fonts.js</c> narrows this list to what the browser
/// can actually render before it is shown. That check is what lets the railway faces be listed at all —
/// hardly anybody has them, and a list of thirteen dead entries would be worse than no list.
/// </para>
/// <para>
/// What is stored is the family name alone (see <c>GeneralSettings.ReportFontFamily</c>), so a plan
/// carrying a name no machine here has still opens: <see cref="CssFontFamily"/> appends the generic
/// family of the same group, and the report is set in something of the same character instead of
/// whatever the browser defaults to.
/// </para>
/// </remarks>
public static class ReportFonts
{
    /// <summary>
    /// The stack used when no font has been chosen: the same one the application interface uses, so an
    /// unconfigured report looks exactly as it did before there was a setting.
    /// </summary>
    public const string DefaultStack = "'Helvetica Neue', Helvetica, Arial, sans-serif";

    // Characters that would end the family name and start something else in a CSS declaration, an HTML
    // attribute or an ODF style. A font name never contains them, so removing them costs nothing and
    // means a name read back from a stored plan can be written into markup as it stands.
    private const string Forbidden = "'\"\\;:{}<>()&";

    /// <summary>
    /// The families offered to the planner, ordered by group and then by name — the order they are
    /// listed in.
    /// </summary>
    /// <remarks>
    /// The railway group is the lettering of the railways themselves, and none of it can be assumed
    /// present: Bahnschrift comes with Windows, the rest are downloaded and installed by whoever wants
    /// them. They are listed by the name the installed family carries, which is what the check in
    /// <c>js/fonts.js</c> and the browser both go by — not by the name of the railway.
    /// </remarks>
    public static IReadOnlyList<ReportFont> Candidates { get; } =
    [
        // DIN 1451 grew out of the Prussian state railways' lettering and became the standard for German
        // railway, road and vehicle lettering — so it and its free descendants head the group.
        new("Alte DIN 1451 Mittelschrift", ReportFontGroup.Railway),
        // Microsoft's DIN 1451, and the one entry here most people already have: it comes with Windows.
        new("Bahnschrift", ReportFontGroup.Railway),
        new("D-DIN", ReportFontGroup.Railway),
        // Condensed faces are worth reaching for on a wide timetable, which is why this one is listed
        // beside its parent rather than left out as a variant.
        new("D-DIN Condensed", ReportFontGroup.Railway),
        // Johnston, the London Underground face of 1916, and its openly licensed revivals.
        new("Farringdon", ReportFontGroup.Railway),
        new("Hammersmith One", ReportFontGroup.Railway),
        new("London Tube", ReportFontGroup.Railway),
        // Kinneir and Calvert's British Rail signage face, and its authorised revival.
        new("New Rail Alphabet", ReportFontGroup.Railway),
        new("Overpass", ReportFontGroup.Railway),
        new("Preußische Musterzeichnung IV44", ReportFontGroup.Railway),
        new("Rail Alphabet", ReportFontGroup.Railway),
        new("Railway", ReportFontGroup.Railway),
        new("SJ Sans", ReportFontGroup.Railway),
        // The East German standard, for anyone modelling a Deutsche Reichsbahn layout.
        new("TGL 0-1451 Engschrift", ReportFontGroup.Railway),

        new("Arial", ReportFontGroup.SansSerif),
        new("Arial Narrow", ReportFontGroup.SansSerif),
        new("Calibri", ReportFontGroup.SansSerif),
        new("Candara", ReportFontGroup.SansSerif),
        new("Corbel", ReportFontGroup.SansSerif),
        new("DejaVu Sans", ReportFontGroup.SansSerif),
        new("Gill Sans MT", ReportFontGroup.SansSerif),
        new("Helvetica", ReportFontGroup.SansSerif),
        new("Helvetica Neue", ReportFontGroup.SansSerif),
        new("Liberation Sans", ReportFontGroup.SansSerif),
        new("Lucida Sans Unicode", ReportFontGroup.SansSerif),
        new("Noto Sans", ReportFontGroup.SansSerif),
        new("Open Sans", ReportFontGroup.SansSerif),
        new("Optima", ReportFontGroup.SansSerif),
        new("Roboto", ReportFontGroup.SansSerif),
        new("Roboto Condensed", ReportFontGroup.SansSerif),
        new("Segoe UI", ReportFontGroup.SansSerif),
        new("Tahoma", ReportFontGroup.SansSerif),
        new("Trebuchet MS", ReportFontGroup.SansSerif),
        new("Verdana", ReportFontGroup.SansSerif),

        new("Book Antiqua", ReportFontGroup.Serif),
        new("Cambria", ReportFontGroup.Serif),
        new("Constantia", ReportFontGroup.Serif),
        new("DejaVu Serif", ReportFontGroup.Serif),
        new("Garamond", ReportFontGroup.Serif),
        new("Georgia", ReportFontGroup.Serif),
        new("Liberation Serif", ReportFontGroup.Serif),
        new("Noto Serif", ReportFontGroup.Serif),
        new("Palatino", ReportFontGroup.Serif),
        new("Palatino Linotype", ReportFontGroup.Serif),
        new("Times New Roman", ReportFontGroup.Serif),

        new("Consolas", ReportFontGroup.Monospace),
        new("Courier New", ReportFontGroup.Monospace),
        new("DejaVu Sans Mono", ReportFontGroup.Monospace),
        new("Liberation Mono", ReportFontGroup.Monospace),
        new("Lucida Console", ReportFontGroup.Monospace),
        new("Menlo", ReportFontGroup.Monospace),
        new("Roboto Mono", ReportFontGroup.Monospace),
    ];

    /// <summary>
    /// The stored family name cleaned of anything that cannot appear in one, or an empty string when
    /// nothing is chosen. Use this wherever the bare name is written into a document.
    /// </summary>
    /// <param name="familyName">The name as stored in the layout's settings.</param>
    public static string FamilyName(string? familyName) =>
        string.IsNullOrWhiteSpace(familyName)
            ? ""
            : new string([.. familyName.Trim().Where(c => !char.IsControl(c) && !Forbidden.Contains(c))]);

    /// <summary>
    /// The CSS <c>font-family</c> value for a chosen family: the family itself, then the generic family
    /// of its group so a machine without the font still prints something of the same character.
    /// Falls back to <see cref="DefaultStack"/> when nothing is chosen.
    /// </summary>
    /// <param name="familyName">The name as stored in the layout's settings.</param>
    public static string CssFontFamily(string? familyName) =>
        FamilyName(familyName) is { Length: > 0 } name
            ? $"'{name}', {GenericOf(GroupOf(name))}"
            : DefaultStack;

    /// <summary>
    /// Where the named family is filed. A name that is not in <see cref="Candidates"/> — a plan written
    /// on another machine, say — is treated as sans-serif.
    /// </summary>
    /// <param name="familyName">The name as stored in the layout's settings.</param>
    public static ReportFontGroup GroupOf(string? familyName) =>
        Candidates.FirstOrDefault(f => f.Name.Equals(FamilyName(familyName), StringComparison.OrdinalIgnoreCase))?.Group
            ?? ReportFontGroup.SansSerif;

    // A named fallback ahead of the generic one, because a browser's generic serif/monospace is not
    // always a face anybody would choose to print a timetable in. A railway face falls back with the
    // sans-serifs: every one of them is a grotesque.
    private static string GenericOf(ReportFontGroup group) => group switch
    {
        ReportFontGroup.Serif => "'Times New Roman', Times, serif",
        ReportFontGroup.Monospace => "'Courier New', Courier, monospace",
        _ => "Arial, Helvetica, sans-serif"
    };
}
