namespace Tellurian.Trains.Schedules.Model.Schedules;
/// <summary>
/// These option applies to motorised units, i.e. locomotives and trainsets.
/// </summary>
public sealed class TractionOptions : TrainPartOptions
{
    /// <summary>
    /// Number of traction uints, e.g. locomotives in consist.
    /// </summary>
    public int NumberOfUnits { get; set; } = 1;
    /// <summary>
    /// If true, should present a note to drivers/stations to use this specific traction unit.
    /// Applies to departures.
    /// </summary>
    public bool DisplayUseNote { get; set; } = true;

    /// <summary>
    /// Where the traction unit comes from before departure: from stabling, lifted on to the track, or
    /// (<see cref="TractionLayover.None"/>) from a track of the station. Anything but none overrides the
    /// <see cref="DisplayUseNote"/> with an instruction to bring the unit to the departure track.
    /// </summary>
    /// <remarks>Must match the <see cref="ToLayover"/> of the part the unit worked before (rule S6).</remarks>
    public TractionLayover FromLayover { get; set; }

    /// <summary>
    /// Where the traction unit goes after arrival: to stabling, lifted off the track, or
    /// (<see cref="TractionLayover.None"/>) it stays on a track of the station.
    /// </summary>
    /// <remarks>Must match the <see cref="FromLayover"/> of the part the unit works next (rule S6).</remarks>
    public TractionLayover ToLayover { get; set; }

    /// <summary>
    /// If true, the traction unit should be turned on arrival, so it faces the other way. Only asked for
    /// where there is a turntable to turn it on (see <see cref="Layouts.Station.HasTurntable"/>).
    /// </summary>
    /// <remarks>If both <see cref="TurnLoco"/> and <see cref="RunaroundLoco"/> apply, they give one note,
    /// 'turn and circulate', rather than two.</remarks>
    public bool TurnLoco { get; set; }

    /// <summary>
    /// If true, the traction unit should be run round to the other end of the train on arrival, so the
    /// train can leave in the opposite direction to the one it arrived from.
    /// </summary>
    /// <remarks>
    /// <para>Ignored where the traction working the part reverses as it stands — a trainset, or a
    /// locomotive working a reversible train — since there is then nothing to run round; see
    /// <c>ScheduledTrainPart.NeedsRunaround</c>.</para>
    /// <para>If both <see cref="TurnLoco"/> and <see cref="RunaroundLoco"/> apply, they give one note,
    /// 'turn and circulate', rather than two.</para>
    /// </remarks>
    public bool RunaroundLoco { get; set; }

    /// <summary>
    /// This traction unit is additional to the primary traction unit, pulling or pushing.
    /// </summary>
    public bool IsReinforcement { get; set; }
}
