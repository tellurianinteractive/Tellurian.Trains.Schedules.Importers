namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>The stretches that the lines of a <see cref="Layout"/> give, and what stopped others from being found.</summary>
/// <param name="Stretches">The stretches, each once, in the order of the lines.</param>
/// <param name="Problems">Each problem once.</param>
public sealed record LineStretchesResult(IReadOnlyList<LineStretch> Stretches, IReadOnlyList<LineProblem> Problems);
