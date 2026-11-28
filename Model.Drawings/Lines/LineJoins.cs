using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>
/// Which exit of a placed module is joined to which, as the lines of a <see cref="DrawnLayout"/> say: the line order,
/// <see cref="LineEntry.IsTurned"/>, <see cref="LineEntry.ContinuesFrom"/>, <see cref="Line.StartAt"/> and
/// <see cref="Line.EndAt"/>. References to modules or exits that don't exist, and exits joined twice, are reported and
/// left out.
/// </summary>
internal sealed class LineJoins
{
    private readonly Dictionary<ExitReference, LineJoin> _joins = [];
    private readonly LineProblemLog _log;

    public LineJoins(DrawnLayout layout, LineProblemLog log)
    {
        _log = log;
        Modules = layout.Modules.GroupBy(m => m.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var line in layout.Lines) AddJoins(line);
    }

    /// <summary>The placed modules by their <see cref="PlacedModule.Id"/>.</summary>
    public IReadOnlyDictionary<int, PlacedModule> Modules { get; }

    /// <summary>Every joint, once from each of its two exits.</summary>
    public IEnumerable<KeyValuePair<ExitReference, LineJoin>> All => _joins;

    /// <summary>Every joint once, seen from the exit the line runs from.</summary>
    public IEnumerable<KeyValuePair<ExitReference, LineJoin>> Forward => _joins.Where(j => j.Value.IsForward);

    public bool TryGet(ExitReference exit, [MaybeNullWhen(false)] out LineJoin join) => _joins.TryGetValue(exit, out join);

    public bool IsJoined(ExitReference exit) => _joins.ContainsKey(exit);

    /// <summary>The exits of the module's drawing; a module drawn without exits is taken to have one at each end.</summary>
    public static IReadOnlyList<ExitPlace> ExitsOf(PlacedModule module)
    {
        var drawing = module.Module.Drawing;
        return drawing.Exits.Count == 0
            ? [ExitPlace.Left, ExitPlace.Right]
            : [.. drawing.Exits.Select(e => ExitPlace.Of(drawing, e)).OfType<ExitPlace>()];
    }

    private void AddJoins(Line line)
    {
        var entries = line.Entries;
        if (entries.Count == 0) return;
        if (line.StartAt is { } start) AddJoin(start, ExitReference.To(entries[0].ModuleId, entries[0].Entry), line.Id, 0);
        for (var i = 1; i < entries.Count; i++)
            AddJoin(ExitReference.To(entries[i - 1].ModuleId, entries[i - 1].Exit), ExitReference.To(entries[i].ModuleId, entries[i].Entry), line.Id, i);
        if (line.EndAt is { } end) AddJoin(ExitReference.To(entries[^1].ModuleId, entries[^1].Exit), end, line.Id, entries.Count, isClosing: true);
    }

    private void AddJoin(ExitReference from, ExitReference to, int lineId, int position, bool isClosing = false)
    {
        if (!IsValid(from) | !IsValid(to)) return;
        if (_joins.ContainsKey(from) || _joins.ContainsKey(to))
        {
            var twice = _joins.ContainsKey(from) ? from : to;
            _log.Report(new(LineProblemCode.ExitJoinedTwice, twice.ModuleId, twice.Place));
            return;
        }
        _joins[from] = new(to, lineId, position, true, isClosing);
        _joins[to] = new(from, lineId, position, false, isClosing);
    }

    private bool IsValid(ExitReference exit)
    {
        if (!Modules.TryGetValue(exit.ModuleId, out var module))
        {
            _log.Report(new(LineProblemCode.ModuleMissing, exit.ModuleId));
            return false;
        }
        if (ExitsOf(module).Contains(exit.Place)) return true;
        _log.Report(new(LineProblemCode.ExitMissing, exit.ModuleId, exit.Place));
        return false;
    }
}
