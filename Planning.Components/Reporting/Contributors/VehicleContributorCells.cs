using System.Globalization;
using Tellurian.Trains.Schedules.Planning.App.Translations;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// The text of each cell of the Vehicle contributors report.
/// </summary>
/// <param name="translator">Translates labels and the vehicle types.</param>
/// <param name="settings">Whether the layout counts in sessions or days, and the names of the days.</param>
public sealed class VehicleContributorCells(Translator translator, SessionsSettings settings)
{
    /// <summary>What is printed where nobody brings the item.</summary>
    public const string Nobody = "—";

    /// <summary>
    /// The heading of the DCC address column: the abbreviation alone, the same in every language, which says it
    /// and keeps the column as narrow as the addresses in it.
    /// </summary>
    public const string DccHeading = "DCC";

    /// <summary>What is printed once across the start columns of an item given no work yet.</summary>
    public string NotInOperation => translator("NotInOperation");

    /// <summary>
    /// The text of one cell. The notes come as one text per note; everything else as a single text.
    /// </summary>
    /// <param name="line">The row.</param>
    /// <param name="column">The column.</param>
    public IReadOnlyList<string> TextsOf(VehicleContributorLine line, VehicleContributorsColumn column) =>
        column == VehicleContributorsColumn.Note ? line.Notes : [TextOf(line, column)];

    /// <summary>
    /// The text of one cell as a single text; the notes are joined, which is how they are read out rather than
    /// printed.
    /// </summary>
    /// <param name="line">The row.</param>
    /// <param name="column">The column.</param>
    public string TextOf(VehicleContributorLine line, VehicleContributorsColumn column) => column switch
    {
        VehicleContributorsColumn.Role => line.Contributor is null
            ? string.Empty
            : translator(line.IsPrimary ? "PrimaryContributor" : "SpareContributor"),
        VehicleContributorsColumn.DccAddress => DccAddressOf(line),
        VehicleContributorsColumn.FirstSession => line.Start is { } start
            ? SessionsFormatting.PositionTextOf(start.FirstPosition, settings, useShortDayName: false)
            : string.Empty,
        VehicleContributorsColumn.Station => line.Start?.Location.Name ?? string.Empty,
        VehicleContributorsColumn.Track => line.Start?.Track.Number ?? string.Empty,
        VehicleContributorsColumn.Departure => line.Start?.Departure.HHMM() ?? string.Empty,
        VehicleContributorsColumn.Type => TypeOf(line.Vehicle),
        // A wagonset listing its wagons gives each of their classes once, and counts the wagons.
        VehicleContributorsColumn.Class => line.Vehicle.Classes,
        // The number alone: the operator and class the designation also holds are in the columns beside it.
        VehicleContributorsColumn.Turnus => line.Vehicle.Number > 0
            ? line.Vehicle.Number.ToString("D2", CultureInfo.CurrentCulture)
            : string.Empty,
        // As in the Vehicle owners tab: a single unit is what nearly every row is, so only a count worth
        // noticing is printed.
        VehicleContributorsColumn.Count => line.Vehicle.NumberOfUnits > 1
            ? line.Vehicle.NumberOfUnits.ToString(CultureInfo.CurrentCulture)
            : string.Empty,
        VehicleContributorsColumn.Owner => line.OwnerName ?? Nobody,
        _ => string.Join(" ", line.Notes),
    };

    /// <summary>The label heading a column.</summary>
    /// <param name="column">The column.</param>
    public string HeadingOf(VehicleContributorsColumn column) => column switch
    {
        VehicleContributorsColumn.DccAddress => DccHeading,
        VehicleContributorsColumn.Role => translator("Role"),
        VehicleContributorsColumn.FirstSession => translator(settings.UseDaysInsteadOfSessionNumbers ? "FirstDay" : "FirstSession"),
        _ => translator(column.ToString()),
    };

    // The kind of vehicle, and after it how a traction unit is powered, as in "Trainset, diesel". The traction type
    // is written as it reads inside a text — lower case, bar the German nouns — so it has labels of its own. One not
    // stated is left out: its label is "Any", which reads on paper as a unit that runs on anything rather than one
    // nobody has said. A wagonset is not powered at all.
    private string TypeOf(ScheduledObject vehicle)
    {
        var type = translator(vehicle.ObjectType.ToString());
        return vehicle.TractionType switch
        {
            TractionType.None or TractionType.Undefined => type,
            var traction => $"{type}, {translator($"{traction}InText")}",
        };
    }

    // Only a traction unit has one; for anything else the cell stays empty rather than saying "none", which
    // would read as something missing. On paper an address the owner is still to provide is as good as none: it
    // is missing either way, and that is what the sheet has to make someone do something about.
    private string DccAddressOf(VehicleContributorLine line) => line switch
    {
        { IsDccAddressMissing: true } => translator("Missing"),
        { Contributor.DccAddress: { } address } => address.ToString(CultureInfo.CurrentCulture),
        _ => string.Empty,
    };
}
