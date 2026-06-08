using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

public sealed class ScheduleState
{
    public event Action? OnChanged;

    private Schedule? _schedule;
    private TimetableStretch? _selectedStretch;

    public Schedule? Schedule
    {
        get => _schedule;
        set { _schedule = value; _selectedStretch = FirstStretch; NotifyChanged(); }
    }

    public TimetableStretch? SelectedStretch
    {
        get => _selectedStretch;
        set { _selectedStretch = value; NotifyChanged(); }
    }

    public bool HasSchedule => _schedule is not null;

    public IReadOnlyList<TimetableStretch> TimetableStretches =>
        _schedule?.Timetable?.Layout?.TimetableStretches is { } stretches
            ? [.. stretches]
            : [];

    public Timetable? Timetable => _schedule?.Timetable;

    public Tellurian.Trains.Schedules.Model.Layout? Layout => _schedule?.Timetable?.Layout;

    private TimetableStretch? FirstStretch =>
        TimetableStretches.Count > 0 ? TimetableStretches[0] : null;

    /// <summary>
    /// Raises <see cref="OnChanged"/> so all subscribing views (e.g. split-pane panes)
    /// re-render. Call this after mutating the shared model through bound properties
    /// that do not notify on their own (settings, train edits, etc.).
    /// </summary>
    public void NotifyChanged() => OnChanged?.Invoke();
}
