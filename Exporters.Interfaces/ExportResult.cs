using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Exporters.Interfaces;

/// <summary>
/// The result of an export: what was written, or messages saying why it couldn't be. The same shape as
/// <c>ImportResult&lt;T&gt;</c> in Importers.Interfaces, with <see cref="Message"/>s that may carry a
/// <see cref="Message.Code"/> for the app to translate.
/// </summary>
/// <typeparam name="T">The type of what was exported.</typeparam>
public readonly struct ExportResult<T>
{
    /// <summary>An empty, failed result.</summary>
    public ExportResult() { }

    /// <summary>A successful export of <paramref name="item"/>, as it was written.</summary>
    public static ExportResult<T> Success(T item) => new() { Item = item, IsSuccess = true };

    /// <summary>A successful export of <paramref name="item"/>, with messages such as warnings.</summary>
    public static ExportResult<T> Success(T item, IEnumerable<Message> messages) => new() { Item = item, IsSuccess = true, Messages = [.. messages] };

    /// <summary>A failed export, with <paramref name="message"/> saying why.</summary>
    public static ExportResult<T> Failure(Message message) => new() { Messages = [message] };

    /// <summary>A failed export, with <paramref name="messages"/> saying why.</summary>
    public static ExportResult<T> Failure(IEnumerable<Message> messages) => new() { Messages = [.. messages] };

    /// <summary>The messages of the export: why it failed, or warnings of a successful one.</summary>
    public Message[] Messages { get; init; } = [];

    /// <summary>What was written, or default if the export failed.</summary>
    public T? Item { get; init; }

    /// <summary>Whether the export succeeded.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Whether the export failed.</summary>
    public bool IsFailure => !IsSuccess;
}
