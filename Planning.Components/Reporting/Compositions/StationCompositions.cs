namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

/// <summary>
/// One station's train compositions: every departure leaving with cargo flow wagons or a wagonset, and every arrival uncoupling cargo flow wagons, in track order and then in time order.
/// </summary>
/// <param name="Station">The station the compositions are for.</param>
/// <param name="Departures">The departures, grouped by track in the station's track order.</param>
public sealed record StationCompositions(OperationLocation Station, IReadOnlyList<CompositionDeparture> Departures)
{
    /// <summary>
    /// The neighbours that lie to the left of the station: those a train departs for against the direction of the track stretch. Named over the
    /// left end of the compositions, so the reader can tell which of the two sheets is the one for the side
    /// of the tracks they stand on.
    /// </summary>
    public IReadOnlyList<OperationLocation> LeftNeighbours { get; init; } = [];

    /// <summary>The neighbours that lie to the right of the station; see <see cref="LeftNeighbours"/>.</summary>
    public IReadOnlyList<OperationLocation> RightNeighbours { get; init; } = [];

    /// <summary>Builds the compositions for one station from the trains of a timetable.</summary>
    /// <remarks>
    /// Track order first, because the staff work the station a track at a time: the sheet is read standing
    /// at the track, and the trains leaving from it follow one another there in time order.
    /// </remarks>
    /// <param name="station">The station to build the compositions for.</param>
    /// <param name="trains">The timetable's trains.</param>
    /// <param name="settings">How sessions are written, and how many there are.</param>
    /// <param name="plan">The plan whose schedules say which wagonsets work the trains. Omit it and only cargo
    /// flows are shown.</param>
    /// <param name="orientation">Where the locations lie left and right, which sets the trains' headings and
    /// the neighbours named at either end. Omit it and every train travels leftwards.</param>
    public static StationCompositions Create(
        OperationLocation station, IEnumerable<Train> trains, SessionsSettings settings, Plan? plan = null,
        CompositionOrientation? orientation = null)
    {
        station = station.ValueOrException(nameof(station));
        trains = trains.ValueOrException(nameof(trains));

        var tracks = station.TracksInDisplayOrder;
        var departures = trains
            .SelectMany(train => CompositionDeparture.Build(train, station, settings, plan, orientation))
            .OrderBy(departure => TrackOrder(tracks, departure.Track))
            .ThenBy(departure => departure.Time)
            .ThenBy(departure => departure.IsArrival ? 0 : 1)
            .ThenBy(departure => departure.TrainIdentity, StringComparer.CurrentCulture)
            .ToList();

        if (orientation is null) return new StationCompositions(station, departures);
        return new StationCompositions(station, departures)
        {
            LeftNeighbours = orientation.NeighboursTowards(station, CompositionHeading.Leftwards),
            RightNeighbours = orientation.NeighboursTowards(station, CompositionHeading.Rightwards),
        };
    }

    // A track the station does not list sorts last rather than first, where it would stand out as the most
    // important one.
    private static int TrackOrder(IReadOnlyList<StationTrack> tracks, StationTrack track)
    {
        for (var i = 0; i < tracks.Count; i++)
            if (tracks[i].Equals(track)) return i;
        return int.MaxValue;
    }
}
