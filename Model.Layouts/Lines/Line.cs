using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Modules of a <see cref="Layout"/> joined end to end, in order (HP-3). A line without <see cref="StartAt"/> is a main
/// line: it and the branches that start from its modules, and from theirs, are one connected group, placed in a hall by
/// one anchor. A branch starts at a free exit of a module in another line (HP-19); a line can end by joining a free exit
/// elsewhere, as a rejoining branch or a closed loop does (HP-20).
/// </summary>
public sealed class Line : LayoutPart
{
    /// <summary>The identity of the line, unique among the lines of the layout; given by <c>layout.Add(line)</c>.</summary>
    public int Id { get; set; }

    /// <summary>What the line is called, if anything.</summary>
    public string? Name { get; set; }

    /// <summary>The modules in order.</summary>
    public List<LineEntry> Entries { get; set; } = [];

    /// <summary>The free exit of a module in another line where this branch starts; null for a main line.</summary>
    public ExitReference? StartAt { get; set; }

    /// <summary>The free exit that the last module is joined to; its closure gap is reported (VA-5).</summary>
    public ExitReference? EndAt { get; set; }

    /// <summary>Whether the line is a branch: it starts at a module of another line.</summary>
    [JsonIgnore]
    public bool IsBranch => StartAt is not null;

    /// <summary>A copy with copies of its entries.</summary>
    public override Line Copy()
    {
        var copy = (Line)base.Copy();
        copy.Entries = [.. Entries.Select(e => e.Copy())];
        return copy;
    }

    /// <inheritdoc/>
    public override string ToString() => Name ?? Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
