using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

/// <summary>
/// A node in the dockable Workspace layout tree. Either a <see cref="DockSplit"/>
/// (two children divided by a draggable splitter) or a <see cref="DockLeaf"/>
/// (a single docked view). Mutable so ratios can be two-way bound and the tree
/// edited in place; serialised polymorphically to localStorage.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(DockSplit), "Split")]
[JsonDerivedType(typeof(DockLeaf), "Leaf")]
public abstract class DockNode
{
    /// <summary>Base constructor.</summary>
    protected DockNode() { }
}

/// <summary>A leaf holding a single view, identified by its tab <see cref="ViewId"/> (the tab href).</summary>
public sealed class DockLeaf : DockNode
{
    /// <summary>Parameterless constructor for JSON deserialization.</summary>
    [JsonConstructor]
    public DockLeaf() { }

    /// <summary>Creates a leaf for the given view id.</summary>
    public DockLeaf(string viewId) => ViewId = viewId;

    /// <summary>The docked view's id (the <see cref="TabRegistry"/> href).</summary>
    public string ViewId { get; set; } = string.Empty;
}

/// <summary>A split dividing two child nodes by a draggable splitter.</summary>
public sealed class DockSplit : DockNode
{
    /// <summary>Parameterless constructor for JSON deserialization.</summary>
    [JsonConstructor]
    public DockSplit() { }

    /// <summary>Orientation of the divider (Horizontal = side by side, Vertical = stacked).</summary>
    public SplitOrientation Orientation { get; set; }

    /// <summary>Fraction of space given to <see cref="First"/> (0.1–0.9).</summary>
    public double Ratio { get; set; } = 0.5;

    /// <summary>The first child (left when Horizontal, top when Vertical).</summary>
    public DockNode First { get; set; } = default!;

    /// <summary>The second child (right when Horizontal, bottom when Vertical).</summary>
    public DockNode Second { get; set; } = default!;
}

/// <summary>Pure operations over a <see cref="DockNode"/> tree.</summary>
public static class DockTree
{
    /// <summary>Whether the tree contains a leaf for <paramref name="viewId"/>.</summary>
    public static bool ContainsView(DockNode? node, string viewId) => node switch
    {
        DockLeaf leaf => leaf.ViewId == viewId,
        DockSplit split => ContainsView(split.First, viewId) || ContainsView(split.Second, viewId),
        _ => false
    };

    /// <summary>Enumerates the view ids of all leaves, left/top to right/bottom.</summary>
    public static IEnumerable<string> EnumerateViewIds(DockNode? node)
    {
        switch (node)
        {
            case DockLeaf leaf:
                yield return leaf.ViewId;
                break;
            case DockSplit split:
                foreach (var v in EnumerateViewIds(split.First)) yield return v;
                foreach (var v in EnumerateViewIds(split.Second)) yield return v;
                break;
        }
    }

    /// <summary>
    /// Removes the leaf for <paramref name="viewId"/>, collapsing its parent split by
    /// promoting the sibling. Returns the new root (null if the tree becomes empty).
    /// </summary>
    public static DockNode? RemoveLeaf(DockNode? node, string viewId)
    {
        switch (node)
        {
            case null:
                return null;
            case DockLeaf leaf:
                return leaf.ViewId == viewId ? null : leaf;
            case DockSplit split:
                var first = RemoveLeaf(split.First, viewId);
                var second = RemoveLeaf(split.Second, viewId);
                if (first is null) return second;   // promote sibling
                if (second is null) return first;
                split.First = first;
                split.Second = second;
                return split;
            default:
                return node;
        }
    }

    /// <summary>
    /// Replaces the leaf for <paramref name="targetViewId"/> with a new split that combines it
    /// with a new leaf for <paramref name="droppedViewId"/>, placed per <paramref name="direction"/>.
    /// Returns the new root.
    /// </summary>
    public static DockNode DockRelativeToLeaf(DockNode root, string targetViewId, string droppedViewId, DropDirection direction) =>
        Replace(root, targetViewId, target => MakeSplit(target, new DockLeaf(droppedViewId), direction));

    /// <summary>
    /// Docks a new leaf for <paramref name="droppedViewId"/> against the whole tree at the given edge.
    /// If <paramref name="root"/> is null, returns just the new leaf.
    /// </summary>
    public static DockNode DockAtRootEdge(DockNode? root, string droppedViewId, DropDirection direction) =>
        root is null ? new DockLeaf(droppedViewId) : MakeSplit(root, new DockLeaf(droppedViewId), direction);

    /// <summary>Removes leaves whose view id is not in <paramref name="knownViewIds"/>; returns the pruned root.</summary>
    public static DockNode? PruneUnknown(DockNode? node, ISet<string> knownViewIds)
    {
        switch (node)
        {
            case null:
                return null;
            case DockLeaf leaf:
                return knownViewIds.Contains(leaf.ViewId) ? leaf : null;
            case DockSplit split:
                var first = PruneUnknown(split.First, knownViewIds);
                var second = PruneUnknown(split.Second, knownViewIds);
                if (first is null) return second;
                if (second is null) return first;
                split.First = first;
                split.Second = second;
                return split;
            default:
                return node;
        }
    }

    private static DockNode Replace(DockNode node, string targetViewId, Func<DockNode, DockNode> make)
    {
        switch (node)
        {
            case DockLeaf leaf when leaf.ViewId == targetViewId:
                return make(leaf);
            case DockSplit split:
                split.First = Replace(split.First, targetViewId, make);
                split.Second = Replace(split.Second, targetViewId, make);
                return split;
            default:
                return node;
        }
    }

    private static DockSplit MakeSplit(DockNode target, DockLeaf dropped, DropDirection direction) => direction switch
    {
        DropDirection.Left => new DockSplit { Orientation = SplitOrientation.Horizontal, First = dropped, Second = target },
        DropDirection.Right => new DockSplit { Orientation = SplitOrientation.Horizontal, First = target, Second = dropped },
        DropDirection.Top => new DockSplit { Orientation = SplitOrientation.Vertical, First = dropped, Second = target },
        DropDirection.Bottom => new DockSplit { Orientation = SplitOrientation.Vertical, First = target, Second = dropped },
        _ => new DockSplit { Orientation = SplitOrientation.Horizontal, First = target, Second = dropped }
    };
}
