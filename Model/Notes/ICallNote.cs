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
    /// Plain-text rendering of the note.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// HTML/CSS markup rendering of the note.
    /// </summary>
    MarkupString Html { get; }
}
