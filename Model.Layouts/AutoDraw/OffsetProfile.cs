using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>
/// The lateral offset of a side from the main path (positive left), as a piecewise linear function
/// of the distance along the main path. Knots are sorted by distance.
/// </summary>
internal sealed class OffsetProfile
{
    private readonly (double S, double Offset)[] _knots;

    private OffsetProfile(IEnumerable<(double S, double Offset)> knots) => _knots = [.. knots.OrderBy(k => k.S)];

    public IEnumerable<double> KnotDistances => _knots.Select(k => k.S);

    /// <summary>An offset that changes linearly from <paramref name="start"/> to <paramref name="end"/> over <paramref name="length"/>.</summary>
    public static OffsetProfile Linear(double length, double start, double end) => new([(0, start), (length, end)]);

    public static OffsetProfile FromKnots(params (double S, double Offset)[] knots) => new(knots);

    public double At(double s)
    {
        if (s <= _knots[0].S) return _knots[0].Offset;
        for (var i = 1; i < _knots.Length; i++)
        {
            var (s0, o0) = _knots[i - 1];
            var (s1, o1) = _knots[i];
            if (s <= s1) return s1 - s0 < Tolerance.Length ? o1 : o0 + (o1 - o0) * (s - s0) / (s1 - s0);
        }
        return _knots[^1].Offset;
    }

    public OffsetProfile Shifted(double offset) => new(_knots.Select(k => (k.S, k.Offset + offset)));

    public OffsetProfile Negated() => new(_knots.Select(k => (k.S, -k.Offset)));
}
