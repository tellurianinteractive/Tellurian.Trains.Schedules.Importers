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
    /// <remarks>
    /// Zero means any number of axles. It stands beside <see cref="MaxNumberOfWagons"/> rather than
    /// overriding it: a siding may take at most twelve wagons and at most sixteen axles at once, and
    /// which of the two binds depends on the wagons that turn up. Both are printed when both are set.
    /// </remarks>
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
    /// If true, the destination note also names the local destinations: the operation locations whose
    /// cargo is served from the station (see <see cref="OperationLocation.CargoServedFrom"/>).
    /// </summary>
    /// <remarks>
    /// The names are listed rather than summed up in a phrase: <em>"Stilkøbing, Alpha, Beta"</em> tells the
    /// reader where the wagons may go, where <em>"Stilkøbing and local destinations"</em> sent them to look
    /// it up. A station serving nothing is listed on its own.
    /// </remarks>
    public bool AndLocalDestinations { get; set; }

    /// <inheritdoc/>
    public override string ToString() => this.ToText;
}

/// <summary>
/// Provides the renderings of a <see cref="Destination"/>.
/// </summary>
/// <remarks>
/// A destination says two things — where the wagons go and how many may be brought there — and they are
/// rendered separately as well as together, because a report that carries the limit in a column of its
/// own must not repeat it beside the place.
/// </remarks>
public static class DestinationExtensions
{
    extension(Destination destination)
    {
        /// <summary>
        /// Where the wagons go and how many, as plain text.
        /// </summary>
        public string ToText => Combined(destination.PlaceText, destination.MaxLoadText);

        /// <summary>
        /// Markup version of <c>ToText</c> in which regions are rendered as coloured chips.
        /// The location name is encoded; it is planner-entered text embedded in note markup.
        /// </summary>
        public MarkupString ToHtml => new(Combined(destination.PlaceHtml.Value, destination.MaxLoadText));

        /// <summary>
        /// Where the wagons go, without how many may be brought there.
        /// </summary>
        public string PlaceText =>
            Place(destination.NamedLocations.Select(location => location.Name), destination.AndText, destination.Regions);

        /// <summary>
        /// Markup version of <c>PlaceText</c>, with regions as coloured chips.
        /// </summary>
        public MarkupString PlaceHtml =>
            new(Place(destination.NamedLocations.Select(location => WebUtility.HtmlEncode(location.Name)), destination.AndText, destination.RegionsHtml));

        /// <summary>
        /// Where the wagons go without the regions: the named locations and the <em>"and beyond"</em>
        /// qualifier. For listing several destinations together with their regions gathered last
        /// (<see cref="StatedRegions"/>).
        /// </summary>
        public string PlaceTextWithoutRegions =>
            Place(destination.NamedLocations.Select(location => location.Name), destination.AndText, string.Empty);

        /// <summary>
        /// Markup version of <c>PlaceTextWithoutRegions</c>.
        /// </summary>
        public MarkupString PlaceHtmlWithoutRegions =>
            new(Place(destination.NamedLocations.Select(location => WebUtility.HtmlEncode(location.Name)), destination.AndText, string.Empty));

        /// <summary>
        /// The regions the destination includes: its station's regions where it is stated with them, else none.
        /// </summary>
        public IReadOnlyList<Region> StatedRegions =>
            destination.AndRegions ? [.. destination.LocationRegions] : [];

        /// <summary>
        /// The most that may be brought here, as a capacity. Unspecified when the destination takes any
        /// number, which is what an unset limit means; both limits when both are set.
        /// </summary>
        /// <remarks>
        /// Zero is what "no limit" is stored as, so it is turned into the absent value here — once, where
        /// the limit is read — rather than at each place that prints one.
        /// </remarks>
        public TrainCapacity MaxLoad => new()
        {
            Axles = destination.MaxNumberOfAxles > 0 ? destination.MaxNumberOfAxles : null,
            Wagons = destination.MaxNumberOfWagons > 0 ? destination.MaxNumberOfWagons : null,
        };

        /// <summary>
        /// Whether the destination limits how much is brought there.
        /// </summary>
        public bool HasMaxLoad => destination.MaxNumberOfAxles > 0 || destination.MaxNumberOfWagons > 0;

        /// <summary>
        /// The load limits in words, e.g. <em>"Axles × 16 Wagons × 12"</em>. Empty when the destination
        /// takes any number — on paper a printed limit with no figure cannot be told from a forgotten
        /// one. A limit that is not set contributes nothing, so the same rule holds one at a time.
        /// </summary>
        public string MaxLoadText
        {
            get
            {
                string?[] limits =
                [
                    destination.MaxNumberOfAxles > 0 ? NoteText.Format(NoteResources.Axles, destination.MaxNumberOfAxles) : null,
                    destination.MaxNumberOfWagons > 0 ? NoteText.Format(NoteResources.Wagons, destination.MaxNumberOfWagons) : null,
                ];
                // Joined by a space, not a comma: the destinations they belong to are themselves joined
                // with commas, and a further one here would read as the start of another destination.
                return string.Join(" ", limits.OfType<string>());
            }
        }

        /// <summary>
        /// The locations the destination names: the location itself, followed — when it includes its local
        /// destinations — by those whose cargo is served from it, in layout order.
        /// </summary>
        private IEnumerable<OperationLocation> NamedLocations =>
            destination.AndLocalDestinations && destination.Location.Layout is { } layout
                ? layout.OperationLocations.Where(location => destination.Location.Equals(location.CargoServedFrom)).Prepend(destination.Location)
                : [destination.Location];

        // Only a Station has regions; other cargo-serving locations, e.g. an industrial area, have none.
        private IEnumerable<Region> LocationRegions =>
            destination.Location is Station station ? station.Regions : [];

        private string Regions =>
            destination.AndRegions ? string.Join(", ", destination.LocationRegions.Select(r => r.Name)) : string.Empty;

        private string RegionsHtml =>
            destination.AndRegions ? string.Join(", ", destination.LocationRegions.Select(r => r.ToHtml.Value)) : string.Empty;

        /// <summary>
        /// The wording <see cref="Destination.AndBeyond"/> prints in a note — <em>"and beyond"</em>.
        /// </summary>
        /// <remarks>
        /// Exposed for the reports that explain the phrase to a reader. The note resources are internal
        /// to the model, and a key that spelled the phrase out again in a resource of its own would be a
        /// second wording free to drift from the first: the phrase explained is the phrase printed.
        /// </remarks>
        public static string AndBeyondPhrase => NoteResources.AndBeyond;

        private string AndText => destination.AndBeyond ? NoteResources.AndBeyond : string.Empty;
    }

    // Every part but the name is optional, so they are joined rather than interpolated: an absent
    // qualifier must leave neither a double space nor a space before the comma.
    private static string Place(IEnumerable<string> names, string andText, string regions)
    {
        var name = string.Join(", ", names);
        var named = andText.Length == 0 ? name : $"{name} {andText}";
        return regions.Length == 0 ? named : $"{named}, {regions}";
    }

    private static string Combined(string place, string maxLoad) =>
        maxLoad.Length == 0 ? place : $"{place} {maxLoad}";
}
