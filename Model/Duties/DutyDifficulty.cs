namespace Tellurian.Trains.Schedules.Model.Duties;

/// <summary>
/// How demanding a <see cref="DriverDuty"/> is, so a participant can pick one matching their
/// experience. Graded by hand — nothing derives it — and left unset on a duty that has never been
/// graded.
/// </summary>
/// <remarks>
/// The grades are named concepts with fixed meanings rather than an open numeric range, which is why
/// this is an enum and not an <see cref="int"/>. Reports colour the grade, but the colour is never the
/// only carrier of the meaning: booklets are frequently printed in black and white, where green, orange
/// and red all reduce to similar greys, so the number or name is always printed beside it.
/// </remarks>
public enum DutyDifficulty
{
    /// <summary>No extra moments — just drive. Shown in green.</summary>
    Easy = 1,

    /// <summary>Some extra moments beyond driving. Shown in orange.</summary>
    Medium = 2,

    /// <summary>
    /// Often involves shunting, handling wagon cards with waybills and sorting wagons. Shown in red.
    /// </summary>
    Experienced = 3,
}
