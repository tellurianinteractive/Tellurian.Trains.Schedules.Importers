namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// A uniform-grid spatial index of items by bounding box. It finds the candidates for distance
/// and overlap checks quickly (spec NF-2), so only nearby outlines are compared exactly.
/// </summary>
/// <remarks>
/// An item or a query area that would cover more than <see cref="MaxCellsPerBox"/> grid cells, or lies outside the
/// grid's range, isn't spread over cells: such an item is kept in a list that every query checks, and such a query
/// checks every item. So a huge or far-away box costs a linear scan, never a loop over an enormous number of cells.
/// </remarks>
public sealed class SpatialIndex<T> where T : notnull
{
    /// <summary>The largest number of grid cells a box is spread over; a larger box is handled without the grid.</summary>
    public const int MaxCellsPerBox = 4096;

    private readonly Dictionary<(int X, int Y), List<T>> _cells = [];
    private readonly Dictionary<T, BoundingBox> _items = [];
    private readonly HashSet<T> _large = [];

    /// <param name="cellSize">The grid cell size in millimetres; about the size of a typical item works well.</param>
    public SpatialIndex(double cellSize)
    {
        if (!(cellSize > 0) || double.IsInfinity(cellSize)) throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "The cell size must be positive and finite.");
        CellSize = cellSize;
    }

    /// <summary>The grid cell size in millimetres.</summary>
    public double CellSize { get; }
    /// <summary>The number of items in the index.</summary>
    public int Count => _items.Count;

    /// <summary>True if <paramref name="item"/> is in the index.</summary>
    public bool Contains(T item) => _items.ContainsKey(item);

    /// <summary>Adds <paramref name="item"/>, or moves it if it is already in the index.</summary>
    /// <exception cref="ArgumentException"><paramref name="bounds"/> is empty or not finite.</exception>
    public void Set(T item, BoundingBox bounds)
    {
        if (bounds.IsEmpty) throw new ArgumentException("An item needs a non-empty bounding box.", nameof(bounds));
        if (!IsFinite(bounds)) throw new ArgumentException("An item needs a finite bounding box.", nameof(bounds));
        Remove(item);
        _items[item] = bounds;
        if (GridCells(bounds) is not { } cells)
        {
            _large.Add(item);
            return;
        }
        foreach (var cell in Enumerate(cells))
        {
            if (!_cells.TryGetValue(cell, out var list)) _cells[cell] = list = [];
            list.Add(item);
        }
    }

    /// <summary>Removes <paramref name="item"/>; false if it wasn't in the index.</summary>
    public bool Remove(T item)
    {
        if (!_items.Remove(item, out var bounds)) return false;
        if (_large.Remove(item)) return true;
        foreach (var cell in Enumerate(GridCells(bounds)!.Value))
        {
            if (_cells.TryGetValue(cell, out var list) && list.Remove(item) && list.Count == 0) _cells.Remove(cell);
        }
        return true;
    }

    /// <summary>Removes all items.</summary>
    public void Clear()
    {
        _cells.Clear();
        _items.Clear();
        _large.Clear();
    }

    /// <summary>The items whose bounding boxes intersect <paramref name="area"/>.</summary>
    public IReadOnlyList<T> Query(BoundingBox area)
    {
        if (area.IsEmpty) return [];
        if (GridCells(area) is not { } cells) return [.. _items.Where(entry => entry.Value.Intersects(area)).Select(entry => entry.Key)];
        var found = new HashSet<T>();
        var result = new List<T>();
        foreach (var cell in Enumerate(cells))
        {
            if (!_cells.TryGetValue(cell, out var list)) continue;
            foreach (var item in list)
            {
                if (found.Add(item) && _items[item].Intersects(area)) result.Add(item);
            }
        }
        foreach (var item in _large)
        {
            if (_items[item].Intersects(area)) result.Add(item);
        }
        return result;
    }

    /// <summary>The items whose bounding boxes are within <paramref name="distance"/> of <paramref name="area"/>.</summary>
    public IReadOnlyList<T> QueryNear(BoundingBox area, double distance) =>
        [.. Query(area.Inflated(distance)).Where(item => Measure.BoxDistance(_items[item], area) <= distance)];

    /// <summary>
    /// The range of grid cells that <paramref name="bounds"/> covers; null when it would be more than
    /// <see cref="MaxCellsPerBox"/> cells or outside the range of cell numbers, and so is handled without the grid.
    /// </summary>
    private (int MinX, int MinY, int MaxX, int MaxY)? GridCells(BoundingBox bounds)
    {
        if (!IsFinite(bounds)) return null;
        var minX = Math.Floor(bounds.Min.X / CellSize);
        var minY = Math.Floor(bounds.Min.Y / CellSize);
        var maxX = Math.Floor(bounds.Max.X / CellSize);
        var maxY = Math.Floor(bounds.Max.Y / CellSize);
        if (minX < int.MinValue || minY < int.MinValue || maxX >= int.MaxValue || maxY >= int.MaxValue) return null;
        if ((maxX - minX + 1) * (maxY - minY + 1) > MaxCellsPerBox) return null;
        return ((int)minX, (int)minY, (int)maxX, (int)maxY);
    }

    private static IEnumerable<(int X, int Y)> Enumerate((int MinX, int MinY, int MaxX, int MaxY) cells)
    {
        for (var x = cells.MinX; x <= cells.MaxX; x++)
        {
            for (var y = cells.MinY; y <= cells.MaxY; y++) yield return (x, y);
        }
    }

    private static bool IsFinite(BoundingBox bounds) =>
        double.IsFinite(bounds.Min.X) && double.IsFinite(bounds.Min.Y) && double.IsFinite(bounds.Max.X) && double.IsFinite(bounds.Max.Y);
}
