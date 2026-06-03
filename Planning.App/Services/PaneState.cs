namespace Tellurian.Trains.Schedules.Planning.App.Services;

public enum SplitOrientation
{
    Horizontal,
    Vertical
}

public sealed class PaneState
{
    public event Action? OnChanged;

    private string? _pinnedTabHref;
    private SplitOrientation _splitOrientation = SplitOrientation.Horizontal;
    private double _splitRatio = 0.5;

    public string? PinnedTabHref
    {
        get => _pinnedTabHref;
        private set { _pinnedTabHref = value; NotifyChanged(); }
    }

    public SplitOrientation SplitOrientation
    {
        get => _splitOrientation;
        set { _splitOrientation = value; NotifyChanged(); }
    }

    public double SplitRatio
    {
        get => _splitRatio;
        set { _splitRatio = Math.Clamp(value, 0.1, 0.9); NotifyChanged(); }
    }

    public bool HasPinnedTab => PinnedTabHref is not null;

    public void PinTab(string href)
    {
        if (_pinnedTabHref == href)
            PinnedTabHref = null;
        else
            PinnedTabHref = href;
    }

    public void UnpinTab() => PinnedTabHref = null;

    public void ToggleOrientation() =>
        SplitOrientation = SplitOrientation == SplitOrientation.Horizontal
            ? SplitOrientation.Vertical
            : SplitOrientation.Horizontal;

    private void NotifyChanged() => OnChanged?.Invoke();
}
