namespace Tellurian.Trains.Schedules.Model;
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
    public bool DisplayUseNote { get; set; }

    /// <summary>
    /// If true, overrides the <see cref="DisplayUseNote"/> with instruction to get traction unit from parking before departure time.
    /// </summary>
    public bool FromParking { get; set; }

    /// <summary>
    /// If true, should present a note to drivers/stations to move traction unit to parking after arrival.
    /// </summary>
    public bool ToParking { get; set; }

    /// <summary>
    /// If true, traction unit should be turned (often using a turntable) after arrival.
    /// </summary>
    /// <remarks>If both <see cref="TurnLoco"/> and <see cref="ReverseLoco"/> are true, this should result in one note 'turn and reverse'.</remarks>
    public bool TurnLoco { get; set; }

    /// <summary>
    /// If true, traction unit should be reversed, e.g. moved from one end of the train to the other in order to continue in opposite direction as arrived.
    /// </summary>
    /// <remarks>If both <see cref="TurnLoco"/> and <see cref="ReverseLoco"/> are true, this should result in one note 'turn and reverse'.</remarks>
    public bool ReverseLoco { get; set; }

    /// <summary>
    /// This traction unit is additional to the primary traction unit, pulling or pushing.
    /// </summary>
    public bool IsReinforcement { get; set; }
}
