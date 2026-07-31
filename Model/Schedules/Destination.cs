using Microsoft.AspNetCore.Components;
using System.Net;
using NoteResources = Tellurian.Trains.Schedules.Model.Resources.Notes;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Descriptor of destinations for freight wagons that a train should bring.
/// </summary>
public class Destination
{
    /// <summary>
    /// The operation location that is the destination. Any location exchanging cargo qualifies
    /// (see <see cref="OperationLocation.HasCargoExchange"/>), not only a <see cref="Layouts.Station"/> —
    /// an <see cref="IndustrialArea"/> is a destination for freight wagons as much as a station is.
    /// </summary>
    public required OperationLocation Location { get; set; }

    /// <summary>
    /// The destinations position in train.
    /// </summary>
    /// <remarks>Several destinations can have the same position. Position zero means anywhere in train.</remarks>
    public int PositionInTrain { get; set; } = 0;

    /// <summary>
    /// The maximum wagons to bring to this destination.
    /// </summary>
    /// <remarks>Zero means any number of wagons.</remarks>
    public int MaxNumberOfWagons { get; set; }

    /// <summary>
    /// The maximum number of total axles for all wagons to bring to this location.
    /// </summary>
    /// <remarks>Zero means any number of axles. If axles are specified, it overrides any value in <see cref="MaxNumberOfWagons"/></remarks>
    public int MaxNumberOfAxles { get; set; }

    /// <summary>
    /// If true, the destination's note should contain the regions at the location.
    /// </summary>
    public bool AndRegions { get; set; }

    /// <summary>
    /// If true, the destination note should contain 'and beyond', meaning all operation locations in the layout beyond the station.
    /// </summary>
    public bool AndBeyond { get; set; }

    /// <summary>
    /// If true, the destination note should contain 'and local destinations', meaning all operation locations served by freight trains from the station.
    /// </summary>
    public bool AndLocalDestinations { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        HasRegions ?
        $"{Location.Name} {AndText}, {Regions} {MaxLength}".TrimEnd() :
        $"{Location.Name} {AndText} {MaxLength}".TrimEnd();

    /// <summary>
    /// Markup version of <see cref="ToString"/> in which regions are rendered as coloured chips.
    /// The location name is encoded; it is planner-entered text embedded in note markup.
    /// </summary>
    public MarkupString ToHtml => new(
        HasRegions ?
        $"{LocationNameHtml} {AndText}, {RegionsHtml} {MaxLength}".TrimEnd() :
        $"{LocationNameHtml} {AndText} {MaxLength}".TrimEnd());

    private string LocationNameHtml => WebUtility.HtmlEncode(Location.Name);

    // Only a Station has regions; other cargo-serving locations, e.g. an industrial area, have none.
    private IEnumerable<Region> LocationRegions => Location is Station station ? station.Regions : [];
    private bool HasRegions => AndRegions && LocationRegions.Any();

    private string Regions => AndRegions ? string.Join(", ", LocationRegions.Select(r => r.Name)) : string.Empty;
    private string RegionsHtml => AndRegions ? string.Join(", ", LocationRegions.Select(r => r.ToHtml.Value)) : string.Empty;
    private string AndText =>
        AndLocalDestinations && AndBeyond ? NoteResources.AndLocalDestinationsAndBeyond :
        AndBeyond ? NoteResources.AndBeyond :
        AndLocalDestinations ? NoteResources.AndLocalDestinations :
        string.Empty;

    private string MaxLength =>
        MaxNumberOfAxles > 0 ? NoteText.Format(NoteResources.Axles, MaxNumberOfAxles) :
        MaxNumberOfWagons > 0 ? NoteText.Format(NoteResources.Wagons, MaxNumberOfWagons) :
        string.Empty;
}
