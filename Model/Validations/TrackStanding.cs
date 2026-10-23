namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// One occupant of a station track for a stretch of time, as the track capacity rule (L5) sees it: a
/// train's call, or the vehicles of a schedule standing between their trains (a <see cref="TrackHold"/>).
/// </summary>
/// <param name="Track">The track the occupant stands on.</param>
/// <param name="From">When it takes the track.</param>
/// <param name="To">When it leaves it.</param>
/// <param name="Sessions">The sessions or days it applies to.</param>
/// <param name="Length">Its length in metres; zero where it is not known, which keeps it out of rule L5.</param>
/// <param name="Name">What messages call it: the train, or the vehicles standing there.</param>
/// <param name="Train">The train it belongs to, or the train that brought the vehicles.</param>
/// <param name="Call">The call, for a train; <c>null</c> for standing vehicles.</param>
/// <param name="Hold">The stay, for standing vehicles; <c>null</c> for a train.</param>
/// <param name="Entry">The end of the track it comes in by, or <c>null</c> where that is not known.</param>
/// <param name="Exit">The end of the track it goes out by, or <c>null</c> where that is not known.</param>
internal sealed record TrackStanding(
    StationTrack Track,
    Time From,
    Time To,
    Sessions Sessions,
    double Length,
    object Name,
    Train Train,
    StationCall? Call,
    TrackHold? Hold,
    int? Entry,
    int? Exit)
{
    /// <summary>The span the occupant is on the track.</summary>
    public (Time From, Time To) Span => (From, To);

    /// <summary>
    /// Whether the occupant counts in rule L5: the track has a length and so has the occupant. What does
    /// not is left to the plain double-booking rule (L2).
    /// </summary>
    public bool IsMeasured => Track.Length > 0 && Length > 0;
}

/// <summary>
/// Collects what stands on a station track, for the capacity rule (L5).
/// </summary>
internal static class TrackStandings
{
    /// <summary>The end every vehicle uses on a stub track, which has only the one.</summary>
    private const int StubEnd = -1;

    /// <summary>
    /// The length a call stands for: the vehicles of the schedules that work it, added up; where none of
    /// them has a length, the metre limit of the train; zero where there is neither.
    /// </summary>
    public static double StandingLength(this StationCall call, IEnumerable<Schedule> schedules)
    {
        var vehicles = schedules
            .Where(s => s.Parts.Any(p => p.ContainsCall(call)))
            .SelectMany(s => s.Vehicles)
            .Distinct()
            .Sum(v => v.Length);
        return vehicles > 0 ? vehicles : call.Train?.Length.Meters ?? 0;
    }

    /// <summary>The length of the vehicles standing in a hold.</summary>
    public static double StandingLength(this TrackHold hold) =>
        (hold.Group.Count > 0 ? hold.Group : [.. hold.Schedule.Vehicles]).Sum(v => v.Length);

    /// <summary>
    /// Everything that stands on <paramref name="track"/>: its calls and, given the holds, the vehicles
    /// standing between their trains.
    /// </summary>
    /// <param name="track">The track.</param>
    /// <param name="schedules">The schedules that assign vehicles to train parts.</param>
    /// <param name="holds">The stays of vehicles on this track; empty where they do not count.</param>
    public static IReadOnlyList<TrackStanding> On(StationTrack track, IEnumerable<Schedule> schedules, IEnumerable<TrackHold> holds)
    {
        int? End(int? end) => track.IsStub ? StubEnd : end;
        var result = new List<TrackStanding>();
        foreach (var call in track.Calls)
        {
            if (call.Train is not { } train || !call.OccupiesTrack(schedules)) continue;
            result.Add(new TrackStanding(track, call.Arrival, call.Departure, train.Sessions, call.StandingLength(schedules),
                train, train, call, null, End(call.NeighbourLocation(before: true)), End(call.NeighbourLocation(before: false))));
        }
        foreach (var hold in holds)
            result.Add(new TrackStanding(track, hold.From, hold.To, hold.Sessions, hold.StandingLength(),
                hold.Vehicles, hold.Train, null, hold, End(hold.EntryEnd), End(hold.ExitEnd)));
        return result;
    }
}
