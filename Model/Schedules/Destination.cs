using Microsoft.AspNetCore.Components;
using NoteResources = Tellurian.Trains.Schedules.Model.Resources.Notes;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Descriptor of destinations for freight wagons that a train should bring.
/// </summary>
public class Destination
{
    /// <summary>
    /// Station that is destination.
    /// </summary>
    public required Station Station { get; set; }

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
    /// The maximum number of total axles for all wagons to bring to this station.
    /// </summary>
    /// <remarks>Zero means any number of axles. If axles are specified, it overrides any value in <see cref="MaxNumberOfWagons"/></remarks>
    public int MaxNumberOfAxles { get; set; }

    /// <summary>
    /// If true, the destination's note should contain the regions at the station.
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
        AndRegions && Station.Regions.Any() ?
        $"{Station.Name} {AndText}, {Regions} {MaxLength}".TrimEnd() :
        $"{Station.Name} {AndText} {MaxLength}".TrimEnd();

    /// <summary>
    /// Markup version of <see cref="ToString"/> in which regions are rendered as coloured chips (see <see cref="Region.Display"/>).
    /// </summary>
    public MarkupString Display => new(
        AndRegions && Station.Regions.Any() ?
        $"{Station.Name} {AndText}, {RegionsHtml} {MaxLength}".TrimEnd() :
        $"{Station.Name} {AndText} {MaxLength}".TrimEnd());

    private string Regions => AndRegions ? string.Join(", ", Station.Regions.Select(r => r.Name)) : string.Empty;
    private string RegionsHtml => AndRegions ? string.Join(", ", Station.Regions.Select(r => r.Display.Value)) : string.Empty;
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
