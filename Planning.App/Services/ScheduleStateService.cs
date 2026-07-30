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
/// A save also broadcasts to other windows on the same origin (<see cref="CrossWindowSyncService"/>),
/// which reload the plan from storage and refresh their own views.
/// </summary>
public sealed class ScheduleStateService(BrowserStorageService storage, CrossWindowSyncService sync, ILogger<ScheduleStateService> logger)
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

    // Debounced persistence: an edit notifies subscribers immediately (cheap, keeps docked views live)
    // but the expensive full-plan serialize is coalesced to run once the user pauses, rather than on
    // every edit. A pending save is flushed on dispose/navigation so the last edit is never dropped.
    private const int SaveDebounceMilliseconds = 750;
    private CancellationTokenSource? _saveDebounceCts;
    private bool _savePending;

    public Plan? Schedule
    {
        get => _schedule;
        set { _schedule = value; _schedule?.Timetable?.RebuildStationCalls(); CancelPendingSave(); ResetSelectionToFirstStretch(); PersistSchedule(); NotifyChanged(); }
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

    /// <summary>
    /// Whether the one-time restore from localStorage has already run (see <see cref="InitializeAsync"/>).
    /// Lets the layout skip the "restoring your work" spinner when it is merely re-created — for example
    /// after viewing a read-only report — rather than on a genuine cold start.
    /// </summary>
    public bool IsInitialized => _loaded;

    public IReadOnlyList<TimetableStretch> TimetableStretches =>
        _schedule?.Timetable?.Layout?.TimetableStretches is { } stretches
            ? [.. stretches.OrderBy(NumericOrderKey).ThenBy(s => s.Number, StringComparer.OrdinalIgnoreCase)]
            : [];

    public Timetable? Timetable => _schedule?.Timetable;

    public Tellurian.Trains.Schedules.Model.Layouts.Layout? Layout => _schedule?.Timetable?.Layout;

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

        sync.OnMessage += OnRemoteScheduleChanged;
        await sync.SubscribeAsync();

        var plan = await storage.GetAsync<Plan>(ScheduleStorageKey, JsonOptions);
        if (plan is null) return;

        _schedule = plan;
        // A persisted plan may carry track calls whose train is no longer in the timetable (an orphan
        // from an older save or import). Reconcile the per-track index with Train.Calls before anything
        // validates or displays it, so orphans cannot surface as phantom conflicts.
        _schedule.Timetable?.RebuildStationCalls();
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

    /// <summary>
    /// Notifies subscribers immediately (so views docked alongside the editor re-render at once) and
    /// schedules a debounced save of the plan. Call after editing the shared model through bound
    /// properties that do not save or notify on their own (settings, train edits). The save is
    /// coalesced across rapid edits; call <see cref="FlushPendingSave"/> when the page may be lost
    /// (dispose, navigation) to guarantee the last edit is written.
    /// </summary>
    public void SaveAndNotify()
    {
        NotifyChanged();
        ScheduleDebouncedSave();
    }

    /// <summary>
    /// Writes any pending debounced save immediately. Call before the editing view is disposed or the
    /// user navigates away, so an edit made within the debounce window is not dropped.
    /// </summary>
    public void FlushPendingSave()
    {
        if (!_savePending) return;
        CancelPendingSave();
        PersistSchedule();
    }

    private void ScheduleDebouncedSave()
    {
        _savePending = true;
        _saveDebounceCts?.Cancel();
        _saveDebounceCts?.Dispose();
        _saveDebounceCts = new CancellationTokenSource();
        _ = RunDebouncedSaveAsync(_saveDebounceCts.Token);
    }

    private async Task RunDebouncedSaveAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(SaveDebounceMilliseconds, token);
        }
        catch (OperationCanceledException)
        {
            return; // superseded by a newer edit, or flushed synchronously
        }
        _savePending = false;
        PersistSchedule();
    }

    private void CancelPendingSave()
    {
        _saveDebounceCts?.Cancel();
        _saveDebounceCts?.Dispose();
        _saveDebounceCts = null;
        _savePending = false;
    }

    private void PersistSchedule()
    {
        if (!_loaded) return; // don't write back while still restoring
        _ = PersistScheduleAsync();
    }

    // Persistence is fire-and-forget so it never blocks the UI, but the task MUST be observed: a
    // throwing serialize (e.g. a model getter that throws) would otherwise be swallowed silently and
    // break persistence with no trace — exactly the failure this logging is here to prevent.
    private async Task PersistScheduleAsync()
    {
        try
        {
            if (_schedule is null)
                await storage.RemoveAsync(ScheduleStorageKey);
            else
                await storage.SetAsync(ScheduleStorageKey, _schedule, JsonOptions);
            await sync.PostAsync(ScheduleStorageKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist the schedule to browser storage.");
        }
    }

    /// <summary>
    /// Applies a schedule saved by another window on the same origin: reloads the plan from storage
    /// and refreshes subscribers. Unlike the <see cref="Schedule"/> setter, this does not reset the
    /// selected stretches or re-persist/re-broadcast, since the change already came from storage.
    /// </summary>
    private async void OnRemoteScheduleChanged(string key)
    {
        if (key != ScheduleStorageKey) return;
        try
        {
            var plan = await storage.GetAsync<Plan>(ScheduleStorageKey, JsonOptions);
            _schedule = plan;
            _schedule?.Timetable?.RebuildStationCalls();
            NotifyChanged();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply a cross-window schedule update.");
        }
    }

    private void PersistSelectedStretches()
    {
        if (!_loaded) return;
        _ = _selectedStretches.Count > 0
            ? storage.SetStringAsync(SelectedStretchesStorageKey, string.Join(',', SelectedStretches.Select(s => s.Number)))
            : storage.RemoveAsync(SelectedStretchesStorageKey);
    }
}
