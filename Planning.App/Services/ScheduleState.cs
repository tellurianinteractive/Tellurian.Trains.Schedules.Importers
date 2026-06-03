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

    private TimetableStretch? FirstStretch =>
        TimetableStretches.Count > 0 ? TimetableStretches[0] : null;

    private void NotifyChanged() => OnChanged?.Invoke();
}
