using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Common contract for all call notes — both the persisted <see cref="CallNote"/> family and the
/// transient <see cref="GeneratedNote"/> family — so that notes from either source can be combined
/// into a single list, sorted by <see cref="DisplayOrder"/>, and rendered as plain text or markup.
/// </summary>
public interface ICallNote
{
    /// <summary>
    /// Controls the order in which the note is displayed; lower values sort first.
    /// </summary>
    int DisplayOrder { get; }

    /// <summary>
    /// Gets a value indicating whether this note is for the driver.
    /// </summary>
    bool IsDriverNote { get; }

    /// <summary>
    /// Gets a value indicating whether this note is for station staff.
    /// </summary>
    bool IsStationNote { get; }

    /// <summary>
    /// Gets a value indicating whether this note is for shunting personnel.
    /// </summary>
    bool IsShuntingNote { get; }

    /// <summary>
    /// Gets a value indicating whether this note belongs to the call's arrival.
    /// </summary>
    /// <remarks>
    /// A call that renders as a single row has nowhere to put this split, so both halves appear
    /// together on that row. A note can therefore be classified by what it describes rather than by
    /// where it happens to be visible.
    /// </remarks>
    bool IsForArrival { get; }

    /// <summary>
    /// Gets a value indicating whether this note belongs to the call's departure. See
    /// <see cref="IsForArrival"/>.
    /// </summary>
    bool IsForDeparture { get; }

    /// <summary>
    /// Plain-text rendering of the note.
    /// </summary>
    string ToText { get; }

    /// <summary>
    /// HTML/CSS markup rendering of the note.
    /// </summary>
    MarkupString ToHtml { get; }
}
