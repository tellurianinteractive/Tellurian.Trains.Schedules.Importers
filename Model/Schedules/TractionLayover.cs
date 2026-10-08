namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Where a traction unit spends the time between two trains when it does not stand on a track of the
/// station: driven away to stabling, or lifted off the layout altogether.
/// </summary>
/// <remarks>
/// A long layover may take a locomotive or trainset out of the way of other trains. On a model railway
/// that is either a stabling siding it is driven to, or a table it is lifted onto and put back on the
/// track from before it leaves again. The two exclude each other, so they are one choice rather than two
/// flags. The arrival of one part and the departure of the next must say the same (rule S6).
/// </remarks>
public enum TractionLayover
{
    /// <summary>The unit stays on a track of the station: the train's own, or one the part names.</summary>
    None = 0,

    /// <summary>The unit is driven to stabling after arrival, or from stabling before departure.</summary>
    Stabling = 1,

    /// <summary>The unit is lifted off the track after arrival, or lifted on before departure.</summary>
    LiftedOff = 2,
}
