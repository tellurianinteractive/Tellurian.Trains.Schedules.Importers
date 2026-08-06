using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// Runs the model's scheduling validations against the currently loaded <see cref="Plan"/> and caches
/// the result, so every GUI component reads one shared error set instead of validating on its own. The
/// recompute is triggered by <see cref="ScheduleStateService.OnChanged"/> and debounced, so rapid edits
/// coalesce into a single pass once the user pauses. Subscribers re-render via <see cref="OnValidationChanged"/>.
/// </summary>
public sealed class ValidationStateService : IDisposable
{
    // An edit fires OnChanged immediately, but validating on every keystroke is wasteful; coalesce to a
    // single pass once the user pauses. Shorter than the persistence debounce: feedback should feel live.
    private const int RecomputeDebounceMilliseconds = 300;

    private readonly ScheduleStateService _schedule;
    private readonly ILogger<ValidationStateService> _logger;
    private CancellationTokenSource? _debounceCts;
    private IReadOnlyList<ValidationError> _errors = [];
    private bool _hasComputed;

    public ValidationStateService(ScheduleStateService schedule, ILogger<ValidationStateService> logger)
    {
        _schedule = schedule;
        _logger = logger;
        _schedule.OnChanged += OnScheduleChanged;
    }

    /// <summary>Raised after the cached error set has been recomputed so subscribing views refresh.</summary>
    public event Action? OnValidationChanged;

    /// <summary>
    /// The current validation errors. Computed lazily on first read (covering a warm start where the
    /// plan was already restored and no change event fires) and refreshed on every schedule change.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors
    {
        get
        {
            if (!_hasComputed) Recompute();
            return _errors;
        }
    }

    /// <summary>The number of current validation errors.</summary>
    public int Count => Errors.Count;

    /// <summary>Whether there are any current validation errors.</summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>The highest severity among the current errors, or <see cref="Severity.None"/> when clean.</summary>
    public Severity MaxSeverity => Errors.Count == 0 ? Severity.None : Errors.Max(e => e.Severity);

    public void Dispose()
    {
        _schedule.OnChanged -= OnScheduleChanged;
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
    }

    private void OnScheduleChanged()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();
        _ = RunDebouncedRecomputeAsync(_debounceCts.Token);
    }

    private async Task RunDebouncedRecomputeAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(RecomputeDebounceMilliseconds, token);
        }
        catch (OperationCanceledException)
        {
            return; // superseded by a newer edit
        }
        Recompute();
        OnValidationChanged?.Invoke();
    }

    // Validation must never take the UI down: a throwing rule is logged and leaves an empty error set,
    // exactly as the persistence path guards its fire-and-forget serialize.
    private void Recompute()
    {
        _hasComputed = true;
        try
        {
            _errors = _schedule.Schedule is { } plan && _schedule.Layout is { } layout
                ? [.. plan.GetValidationErrors(layout.Settings.Validation)]
                : [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation of the schedule failed; clearing the error list.");
            _errors = [];
        }
    }
}
