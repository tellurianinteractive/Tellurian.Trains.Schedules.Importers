namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note saying to circulate the loco — run it round to the other end of the train — so the train can
/// leave in the opposite direction to the one it arrived from.
/// </summary>
/// <remarks>
/// <para>
/// Derived from <see cref="TractionOptions.ReverseLoco"/>. Both the loco driver and the station's
/// dispatcher get it: the driver does the move, and the dispatcher has to give it the road.
/// </para>
/// <para>
/// It names no vehicle. Circulating is a manoeuvre every loco driver knows, and the loco is the one
/// standing in front of them — so the note says what to do and nothing more. That also makes it one note
/// per train part rather than one per traction unit, which is what double-headed traction wants: the
/// whole consist circulates once.
/// </para>
/// </remarks>
public sealed record CirculateNote : GeneratedNote;

/// <summary>
/// Note saying to turn the loco, usually on a turntable, so it faces the other way. Derived from
/// <see cref="TractionOptions.TurnLoco"/>.
/// </summary>
public sealed record TurnNote : GeneratedNote;

/// <summary>
/// Note saying to both turn the loco and circulate it to the other end of the train.
/// </summary>
/// <remarks>
/// One note rather than a <see cref="TurnNote"/> beside a <see cref="CirculateNote"/>, because it is one
/// errand: the loco leaves the train, goes to the turntable and comes back on the other end. Two notes
/// would read as two independent moves.
/// </remarks>
public sealed record TurnAndCirculateNote : GeneratedNote;
