using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>The line between two neighbouring locations of a <see cref="Layout"/>.</summary>
public sealed class TrackStretch : LayoutPart, IEquatable<TrackStretch>
{
    /// <summary>For JSON and EF Core.</summary>
    [JsonConstructor]
    private TrackStretch()
    {
        Start = default!;
        End = default!;
        Layout = default!;
    }

    /// <summary>Creates a single-track stretch with the default speed, and a time in minutes equal to the distance in metres.</summary>
    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance) : this(id, start, end, distance, 1, 100, (int)Math.Round(distance, 0)) { }

    /// <summary>Creates a stretch with the default speed, and a time in minutes equal to the distance in metres.</summary>
    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance, int tracksCount) : this(id, start, end, distance, tracksCount, 100, (int)Math.Round(distance, 0)) { }

    /// <summary>Creates a stretch.</summary>
    /// <param name="id">The identity of the stretch within the layout.</param>
    /// <param name="start">The location at one end.</param>
    /// <param name="end">The location at the other end, of the same layout.</param>
    /// <param name="distance">The distance in metres.</param>
    /// <param name="tracksCount">The number of tracks.</param>
    /// <param name="speed">The maximum speed in km/h.</param>
    /// <param name="time">The travel time in minutes.</param>
    /// <exception cref="ArgumentException">The two locations are not of the same layout.</exception>
    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance, int tracksCount, int speed, int time)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);
        if (!Equals(start.Layout, end.Layout)) throw new ArgumentException($"Both {start} and {end} must be in the same layout.", nameof(end));
        Id = id;
        Start = start;
        End = end;
        StartId = start.Id;
        EndId = end.Id;
        Distance = distance;
        TracksCount = tracksCount;
        Speed = speed;
        Time = time;
        Layout = start.Layout;
        LayoutId = start.Layout?.Id ?? 0;
    }

    /// <summary>The identity of the stretch within the layout, which dispatch and timetable stretches refer to.</summary>
    public int Id { get; set; }

    /// <summary>The <see cref="OperationLocation.Id"/> of the location at one end.</summary>
    public int StartId { get; set; }

    /// <summary>The location at one end. Written as <see cref="StartId"/>.</summary>
    [JsonIgnore]
    public OperationLocation Start { get; set; }

    /// <summary>The side of the start location that the stretch leaves from; as seen from the module's viewing side.</summary>
    public ExitSide StartSide { get; set; }

    /// <summary>The <see cref="OperationLocation.Id"/> of the location at the other end.</summary>
    public int EndId { get; set; }

    /// <summary>The location at the other end. Written as <see cref="EndId"/>.</summary>
    [JsonIgnore]
    public OperationLocation End { get; set; }

    /// <summary>The side of the end location that the stretch arrives at.</summary>
    public ExitSide EndSide { get; set; }

    /// <summary>
    /// The distance from the centre of one location to the centre of the other, in metres. Reports and graphs convert
    /// it to a shown kilometre figure with the layout's distance factor.
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// The distance was entered by the planner and is kept when the layout is updated from its hall plan; otherwise
    /// the distance follows the hall plan.
    /// </summary>
    public bool IsDistanceEntered { get; set; }

    /// <summary>
    /// The distance in metres that the hall plan's drawings gave when the layout was read from it
    /// (<c>LineStretch.Distance</c>), kept beside <see cref="Distance"/> so the planner can show both without
    /// the drawings. Null for a stretch entered by hand, or one the drawings could not measure.
    /// </summary>
    public double? MeasuredDistance { get; set; }

    /// <summary>
    /// The speed in km/h that the modules of the hall plan gave when the layout was read from it
    /// (<c>LineStretch.Speed</c>), kept beside <see cref="Speed"/>. Null for a stretch entered by hand, or one
    /// where no module has a speed limit.
    /// </summary>
    public int? MeasuredSpeed { get; set; }

    /// <summary>The number of tracks of the line.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int TracksCount { get; set; } = 1;

    /// <summary>
    /// The maximum speed on the stretch, km/h; 0 when not given, as for a stretch worked out from a hall plan. A
    /// stretch without a speed is a warning in the planner, and trains run on it at their own speed
    /// (<c>stretch.LimitingSpeed</c>).
    /// </summary>
    public int Speed { get; set; }

    /// <summary>
    /// The speed was entered by the planner and is kept when the layout is updated from its hall plan; otherwise the
    /// speed follows the modules of the stretch (<c>LineStretch.Speed</c>).
    /// </summary>
    public bool IsSpeedEntered { get; set; }

    /// <summary>The travel time on the stretch, minutes.</summary>
    public int Time { get; set; }

    /// <summary>
    /// The time was entered by the planner and is kept when the distance changes; otherwise it is worked out from the
    /// distance and the speed (<c>TrackStretchTiming.RunningMinutes</c>).
    /// </summary>
    public bool IsTimeEntered { get; set; }

    /// <summary>The line has overhead electrification, so electric traction can run on it.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool IsElectrified { get; set; } = true;

    /// <summary>
    /// The <see cref="LayoutModule.Id"/> of the plain-line modules between the two locations, in order from the start to
    /// the end; empty for a stretch entered from scratch.
    /// </summary>
    public IList<int> ModuleIds { get; set; } = [];

    /// <summary>The foreign key of the layout. Not written: the layout owns the stretch.</summary>
    [JsonIgnore]
    public int LayoutId { get; set; }

    /// <summary>The layout the stretch belongs to. Not written: the layout owns the stretch.</summary>
    [JsonIgnore]
    public Layout Layout { get; set; }

    /// <summary>A copy of the stretch, between the same locations.</summary>
    public override TrackStretch Copy()
    {
        var copy = (TrackStretch)base.Copy();
        copy.ModuleIds = [.. ModuleIds];
        return copy;
    }

    /// <inheritdoc/>
    public bool Equals(TrackStretch? other) => other is not null && Equals(Start, other.Start) && Equals(End, other.End);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TrackStretch other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    /// <inheritdoc/>
    public override string ToString() => $"{Start}–{End}";
}

/// <summary>What can be done with a <see cref="TrackStretch"/> itself.</summary>
public static class TrackStretchFacts
{
    /// <summary>
    /// The speed, km/h, taken for a stretch whose <see cref="TrackStretch.Speed"/> is not given: high, so that the
    /// trains' own speeds are what limit them.
    /// </summary>
    public const int AssumedSpeed = 200;

    extension(TrackStretch stretch)
    {
        /// <summary>Whether the stretch has a speed; 0 means it isn't given.</summary>
        public bool HasSpeed => stretch.Speed > 0;

        /// <summary>The speed that limits trains on the stretch: its own, or <see cref="AssumedSpeed"/> when not given.</summary>
        public int LimitingSpeed => stretch.HasSpeed ? stretch.Speed : AssumedSpeed;

        /// <summary>Turns the stretch round, so that it runs from its end to its start. Nothing else changes.</summary>
        public void Reverse()
        {
            (stretch.Start, stretch.End) = (stretch.End, stretch.Start);
            (stretch.StartId, stretch.EndId) = (stretch.EndId, stretch.StartId);
            (stretch.StartSide, stretch.EndSide) = (stretch.EndSide, stretch.StartSide);
        }
    }
}
