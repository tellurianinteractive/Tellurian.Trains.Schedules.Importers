using Tellurian.Trains.Schedules.Model.Drawings;

namespace Tellurian.Trains.Schedules.Model.Drawings.Catalogues;

/// <summary>
/// A gauge a standard allows: e.g. H0 16.5 mm (prototype 1435 mm), H0m 12 mm (1000 mm), H0e 9 mm (750 mm). A track
/// holds only the width; the standard gives the name (decision 13 of <c>CrossAppReview.md</c>).
/// </summary>
/// <param name="Width">The distance between the rails, in millimetres. Unique within a standard.</param>
/// <param name="Name">What the gauge is called, e.g. "H0m"; null when not named.</param>
/// <param name="PrototypeWidth">The prototype's gauge in millimetres, e.g. 1000; null when not stated.</param>
public sealed record TrackGauge(double Width, string? Name = null, double? PrototypeWidth = null) : ExtensibleRecord
{
    /// <summary>The name, or the width in millimetres when there is none.</summary>
    public override string ToString() => Name ?? $"{Width:0.#} mm";
}
