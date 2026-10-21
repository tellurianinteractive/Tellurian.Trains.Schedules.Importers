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
        VehicleContributorsColumn.Role => line.Contributor is null || line.Other is not null
            ? string.Empty
            : translator(line.IsPrimary ? "PrimaryContributor" : "SpareContributor"),
        VehicleContributorsColumn.DccAddress => DccAddressOf(line),
        VehicleContributorsColumn.FirstSession => line.Start is { } start
            ? SessionsFormatting.PositionTextOf(start.FirstPosition, settings, useShortDayName: false)
            : string.Empty,
        VehicleContributorsColumn.Station => line.Location?.Name ?? string.Empty,
        // A vehicle lifted on just before departure is not set up on the track but on a table beside the layout.
        VehicleContributorsColumn.Track => line.Start switch
        {
            { StartsOnTable: true } => translator("OnTable"),
            { } start => start.Track.Number,
            _ => string.Empty,
        },
        VehicleContributorsColumn.Departure => line.Start?.Departure.HHMM() ?? string.Empty,
        VehicleContributorsColumn.Type => line.Other is { } other ? translator(other.Type.ToString()) : TypeOf(line.Vehicle!),
        // The designation as the Schedules tab names the turnus, e.g. "DSB 01 ME": the operator's signature,
        // the number and the class, so no class column of its own is needed.
        VehicleContributorsColumn.Turnus => line.Other?.Identity ?? line.Vehicle!.Designation,
        // Each unit brought, a spare included, has its own; a wagonset's numbers are its wagons', printed with them.
        VehicleContributorsColumn.VehicleNumber => line.Other is not null || line.Vehicle!.TakesVehicleNumber
            ? VehicleNumberOf(line)
            : string.Empty,
        VehicleContributorsColumn.Count => line.Vehicle?.NumberOfUnits.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
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

    // A unit brought without a number is as much something to make the owner provide as a missing DCC address —
    // except an other vehicle, which is often a wagon or a carriage that has none.
    private string VehicleNumberOf(VehicleContributorLine line) => line.Contributor switch
    {
        null => string.Empty,
        { VehicleNumber: { } number } when !string.IsNullOrWhiteSpace(number) => number,
        _ when line.Other is not null => string.Empty,
        _ => translator("Missing"),
    };

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
