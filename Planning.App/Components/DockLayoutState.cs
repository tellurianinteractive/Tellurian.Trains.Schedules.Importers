using System.Text.Json;
using Microsoft.JSInterop;

namespace Tellurian.Trains.Schedules.Planning.App.Components;

/// <summary>
/// Holds and mutates the dockable Workspace layout (a <see cref="DockNode"/> tree),
/// and the transient id of the view currently being dragged. A singleton that all
/// Workspace panes subscribe to via <see cref="OnChanged"/>. The layout is persisted
/// to browser localStorage and restored on startup. Replaces the old PaneState.
/// </summary>
public sealed class DockLayoutState
{
    private const string StorageKey = "planning.dock.layout.v1";

    /// <summary>Raised whenever the layout or drag state changes.</summary>
    public event Action? OnChanged;

    private string? _draggingViewId;
    private IJSRuntime? _js;
    private bool _loaded;

    /// <summary>The root of the layout tree; null when the Workspace is empty.</summary>
    public DockNode? Root { get; private set; }

    /// <summary>The view id currently being dragged, or null. Setting it notifies so panes show drop zones.</summary>
    public string? DraggingViewId
    {
        get => _draggingViewId;
        set { _draggingViewId = value; OnChanged?.Invoke(); }
    }

    /// <summary>Whether <paramref name="viewId"/> is currently docked somewhere in the Workspace.</summary>
    public bool ContainsView(string viewId) => DockTree.ContainsView(Root, viewId);

    /// <summary>The view ids currently docked in the Workspace.</summary>
    public IEnumerable<string> OpenViewIds => DockTree.EnumerateViewIds(Root);

    /// <summary>
    /// Loads the persisted layout from localStorage, dropping any leaves whose view id is no
    /// longer registered. Falls back to an empty Workspace on missing or invalid data.
    /// Call once from the layout's first render; the <see cref="IJSRuntime"/> is reused for saves.
    /// </summary>
    public async Task LoadAsync(IJSRuntime js)
    {
        _js = js;
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            Root = string.IsNullOrWhiteSpace(json)
                ? null
                : DockTree.PruneUnknown(JsonSerializer.Deserialize<DockNode?>(json), KnownViewIds());
        }
        catch (Exception)
        {
            Root = null; // corrupt/incompatible data → empty Workspace
        }
        _loaded = true;
        OnChanged?.Invoke();
    }

    /// <summary>
    /// Docks <paramref name="droppedViewId"/> relative to the pane showing <paramref name="targetViewId"/>.
    /// If the dropped view is already docked it is moved (removed then re-docked).
    /// </summary>
    public void DockRelative(string targetViewId, string droppedViewId, DropDirection direction)
    {
        if (targetViewId == droppedViewId) return; // self-drop: no-op

        if (DockTree.ContainsView(Root, droppedViewId))
            Root = DockTree.RemoveLeaf(Root, droppedViewId);

        // Re-resolve the target by id: removal above may have collapsed a split.
        Root = Root is not null && DockTree.ContainsView(Root, targetViewId)
            ? DockTree.DockRelativeToLeaf(Root, targetViewId, droppedViewId, direction)
            : DockTree.DockAtRootEdge(Root, droppedViewId, direction);

        NotifyChangedAndPersist();
    }

    /// <summary>Docks <paramref name="droppedViewId"/> at the edge of the whole Workspace (used by the empty placeholder).</summary>
    public void DockRoot(string droppedViewId, DropDirection direction)
    {
        if (DockTree.ContainsView(Root, droppedViewId))
            Root = DockTree.RemoveLeaf(Root, droppedViewId);
        Root = DockTree.DockAtRootEdge(Root, droppedViewId, direction);
        NotifyChangedAndPersist();
    }

    /// <summary>Closes (removes) the pane showing <paramref name="viewId"/>, collapsing its parent split.</summary>
    public void CloseView(string viewId)
    {
        Root = DockTree.RemoveLeaf(Root, viewId);
        NotifyChangedAndPersist();
    }

    /// <summary>Sets a split's divider ratio (clamped 0.1–0.9).</summary>
    public void SetRatio(DockSplit split, double ratio)
    {
        split.Ratio = Math.Clamp(ratio, 0.1, 0.9);
        NotifyChangedAndPersist();
    }

    private static HashSet<string> KnownViewIds() => [.. TabRegistry.Tabs.Select(t => t.Href)];

    private void NotifyChangedAndPersist()
    {
        OnChanged?.Invoke();
        if (_loaded) _ = SaveAsync();
    }

    private async Task SaveAsync()
    {
        if (_js is null) return;
        var json = JsonSerializer.Serialize<DockNode?>(Root);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}
