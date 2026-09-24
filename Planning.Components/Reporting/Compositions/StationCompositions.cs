namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

/// <summary>
/// One station's train compositions: every departure leaving with cargo flow wagons or a wagonset whose wagons
/// are listed, in track order and then in departure time order.
/// </summary>
/// <param name="Station">The station the compositions are for.</param>
/// <param name="Departures">The departures, grouped by track in the station's track order.</param>
public sealed record StationCompositions(OperationLocation Station, IReadOnlyList<CompositionDeparture> Departures)
{
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
    public static StationCompositions Create(
        OperationLocation station, IEnumerable<Train> trains, SessionsSettings settings, Plan? plan = null)
    {
        station = station.ValueOrException(nameof(station));
        trains = trains.ValueOrException(nameof(trains));

        var tracks = station.TracksInDisplayOrder;
        var departures = trains
            .SelectMany(train => CompositionDeparture.Build(train, station, settings, plan))
            .OrderBy(departure => TrackOrder(tracks, departure.Track))
            .ThenBy(departure => departure.DepartureTime)
            .ThenBy(departure => departure.TrainIdentity, StringComparer.CurrentCulture)
            .ToList();

        return new StationCompositions(station, departures);
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
