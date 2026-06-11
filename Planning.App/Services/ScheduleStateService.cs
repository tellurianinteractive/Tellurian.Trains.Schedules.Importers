using System.Globalization;
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
    private const string SelectedStretchesStorageKey = "planning.schedule.selectedStretches.v1";

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
    private readonly HashSet<TimetableStretch> _selectedStretches = [];
    private bool _loaded;

    public Plan? Schedule
    {
        get => _schedule;
        set { _schedule = value; ResetSelectionToFirstStretch(); PersistSchedule(); NotifyChanged(); }
    }

    /// <summary>
    /// The timetable stretches currently chosen for graphical display, in numeric order.
    /// More than one may be selected so several graphical schedules can be shown at once.
    /// </summary>
    public IReadOnlyList<TimetableStretch> SelectedStretches =>
        [.. _selectedStretches.OrderBy(NumericOrderKey).ThenBy(s => s.Number, StringComparer.OrdinalIgnoreCase)];

    public bool IsSelected(TimetableStretch stretch) => _selectedStretches.Contains(stretch);

    /// <summary>
    /// Adds or removes a timetable stretch from the displayed selection and notifies subscribers.
    /// </summary>
    public void SetStretchSelected(TimetableStretch stretch, bool selected)
    {
        var changed = selected ? _selectedStretches.Add(stretch) : _selectedStretches.Remove(stretch);
        if (!changed) return;
        PersistSelectedStretches();
        NotifyChanged();
    }

    public bool HasSchedule => _schedule is not null;

    public IReadOnlyList<TimetableStretch> TimetableStretches =>
        _schedule?.Timetable?.Layout?.TimetableStretches is { } stretches
            ? [.. stretches.OrderBy(NumericOrderKey).ThenBy(s => s.Number, StringComparer.OrdinalIgnoreCase)]
            : [];

    public Timetable? Timetable => _schedule?.Timetable;

    public Tellurian.Trains.Schedules.Model.Layout? Layout => _schedule?.Timetable?.Layout;

    private void ResetSelectionToFirstStretch()
    {
        _selectedStretches.Clear();
        if (TimetableStretches.Count > 0) _selectedStretches.Add(TimetableStretches[0]);
    }

    /// <summary>
    /// Sort key that orders stretches by the numeric value of their <see cref="TimetableStretch.Number"/>
    /// when it is numeric; non-numeric numbers sort last (then alphabetically as a tie-break).
    /// </summary>
    private static int NumericOrderKey(TimetableStretch stretch) =>
        int.TryParse(stretch.Number, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : int.MaxValue;

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
        ResetSelectionToFirstStretch();

        var storedNumbers = await storage.GetStringAsync(SelectedStretchesStorageKey);
        if (!string.IsNullOrEmpty(storedNumbers))
        {
            var numbers = storedNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var restored = TimetableStretches
                .Where(s => numbers.Contains(s.Number, StringComparer.OrdinalIgnoreCase))
                .ToArray();
            if (restored.Length > 0)
            {
                _selectedStretches.Clear();
                foreach (var stretch in restored) _selectedStretches.Add(stretch);
            }
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

    private void PersistSelectedStretches()
    {
        if (!_loaded) return;
        _ = _selectedStretches.Count > 0
            ? storage.SetStringAsync(SelectedStretchesStorageKey, string.Join(',', SelectedStretches.Select(s => s.Number)))
            : storage.RemoveAsync(SelectedStretchesStorageKey);
    }
}
