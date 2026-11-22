using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// The changes left from loading a newer revision of the layout a plan was made from: those the planner confirms one by
/// one, and those that can't be applied. Kept for the open plan while the user moves between tabs; it is forgotten when
/// another plan is opened, and isn't saved, since loading the same file again lists the same changes.
/// </summary>
/// <remarks>
/// After each change applied, the plan is compared with the newer layout again, so the list always says what is left:
/// applying one change may settle another, or give a stretch a distance to take at once. The newer layout is only read,
/// never changed, which is what makes comparing it again possible.
/// </remarks>
public sealed class LayoutRevisionState(ScheduleStateService scheduleState)
{
    private Plan? _plan;
    private Layout? _source;
    private HallPlanUpdate? _update;

    /// <summary>Raised when the changes left are different: started, one applied, or closed.</summary>
    public event Action? OnChanged;

    /// <summary>The changes left for the open plan, or none when there are none, or another plan has been opened since.</summary>
    public HallPlanUpdate? Update => _plan is not null && ReferenceEquals(_plan, scheduleState.Schedule) ? _update : null;

    /// <summary>The resource key of what the newer layout was read from: the importer's.</summary>
    public string SourceKey { get; private set; } = string.Empty;

    /// <summary>There are changes left for the open plan, to confirm or to make by hand.</summary>
    public bool HasChangesLeft => Update is { } update && (update.ToConfirm.Count > 0 || update.Manual.Count > 0);

    /// <summary>Starts listing what <paramref name="update"/> left of taking <paramref name="source"/> into <paramref name="plan"/>.</summary>
    public void Start(Plan plan, Layout source, string sourceKey, HallPlanUpdate update)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(update);
        (_plan, _source, _update, SourceKey) = (plan, source, update, sourceKey);
        OnChanged?.Invoke();
    }

    /// <summary>What applying <paramref name="change"/> would do to the plan, without doing it.</summary>
    public LayoutChangePreview Preview(LayoutChange change) =>
        (_plan ?? throw new InvalidOperationException("No changes are listed.")).PreviewChange(change);

    /// <summary>
    /// Applies <paramref name="change"/> as its preview says, saves the plan and compares it with the newer layout again.
    /// Nothing is changed when something blocks it.
    /// </summary>
    public LayoutChangePreview Apply(LayoutChange change)
    {
        if (_plan is null || _source is null) throw new InvalidOperationException("No changes are listed.");
        var preview = _plan.ApplyChange(change);
        if (!preview.IsAllowed) return preview;
        _update = _plan.UpdateFromHallPlan(_source);
        scheduleState.SaveAndNotify();
        OnChanged?.Invoke();
        return preview;
    }

    /// <summary>Stops listing the changes; loading the file again lists them again.</summary>
    public void Close()
    {
        (_plan, _source, _update) = (null, null, null);
        OnChanged?.Invoke();
    }
}
