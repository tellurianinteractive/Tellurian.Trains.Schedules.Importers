namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A joint between two exits as seen from one of them: the other exit, the line it is in, and where.</summary>
/// <param name="Other">The exit at the other side of the joint.</param>
/// <param name="LineId">The line the joint is part of.</param>
/// <param name="Position">The number of the joint in its line: 0 for <see cref="Line.StartAt"/>, then one per entry.</param>
/// <param name="IsForward">The line runs from this exit to <paramref name="Other"/>.</param>
/// <param name="IsClosing">The joint is a line's <see cref="Line.EndAt"/>: it closes a loop or a rejoining line, so it
/// is checked for its closure gap rather than used to place a module.</param>
internal sealed record LineJoin(ExitReference Other, int LineId, int Position, bool IsForward, bool IsClosing = false);
