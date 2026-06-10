using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

/// <summary>
/// Holds the currently loaded <see cref="Plan"/> and the selected timetable stretch, shared
/// across all pages as a singleton. The plan is persisted to browser localStorage and restored
/// on startup, so it survives a language change (which force-reloads the WASM app) and closing
/// and reopening the browser. All subscribing views re-render via <see cref="OnChanged"/>.
/// </summary>
public sealed class ScheduleStateService(BrowserStorageService storage)
{
    private const string ScheduleStorageKey = "planning.schedule.v2";
    private const string SelectedStretchStorageKey = "planning.schedule.selectedStretch.v1";

    /// <summary>
    /// JSON options matching the schedule importer/exporter: the object graph contains reference
    /// cycles, so <see cref="ReferenceHandler.Preserve"/> and a deep max depth are required to round-trip.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    public event Action? OnChanged;

    private Plan? _schedule;
    private TimetableStretch? _selectedStretch;
    private bool _loaded;

    public Plan? Schedule
    {
        get => _schedule;
        set { _schedule = value; _selectedStretch = FirstStretch; PersistSchedule(); NotifyChanged(); }
    }

    public TimetableStretch? SelectedStretch
    {
        get => _selectedStretch;
        set { _selectedStretch = value; PersistSelectedStretch(); NotifyChanged(); }
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
    /// Restores the persisted plan (and previously selected stretch) from browser localStorage.
    /// Call once from the layout's first render; subsequent calls are no-ops. Notifies subscribers
    /// when a plan was restored so views that rendered the empty state refresh.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_loaded) return;
        _loaded = true;

        var plan = await storage.GetAsync<Plan>(ScheduleStorageKey, JsonOptions);
        if (plan is null) return;

        _schedule = plan;
        _selectedStretch = FirstStretch;

        var stretchNumber = await storage.GetStringAsync(SelectedStretchStorageKey);
        if (!string.IsNullOrEmpty(stretchNumber) &&
            TimetableStretches.FirstOrDefault(s => s.Number == stretchNumber) is { } stretch)
        {
            _selectedStretch = stretch;
        }

        NotifyChanged();
    }

    /// <summary>
    /// Raises <see cref="OnChanged"/> so all subscribing views (e.g. split-pane panes)
    /// re-render. Call this after mutating the shared model through bound properties
    /// that do not notify on their own (settings, train edits, etc.).
    /// </summary>
    public void NotifyChanged() => OnChanged?.Invoke();

    private void PersistSchedule()
    {
        if (!_loaded) return; // don't write back while still restoring
        _ = _schedule is null
            ? storage.RemoveAsync(ScheduleStorageKey)
            : storage.SetAsync(ScheduleStorageKey, _schedule, JsonOptions);
    }

    private void PersistSelectedStretch()
    {
        if (!_loaded) return;
        _ = _selectedStretch?.Number is { } number
            ? storage.SetStringAsync(SelectedStretchStorageKey, number)
            : storage.RemoveAsync(SelectedStretchStorageKey);
    }
}
