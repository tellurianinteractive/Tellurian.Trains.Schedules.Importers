namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Represents the result of an export operation, containing the exported item or error messages.
/// </summary>
/// <typeparam name="T">The type of item being exported.</typeparam>
public readonly struct ExportResult<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ExportResult{T}"/> with default values.
    /// </summary>
    public ExportResult() { }

    /// <summary>
    /// Creates a successful export result with the exported item.
    /// </summary>
    /// <param name="item">The exported item.</param>
    /// <returns>A successful <see cref="ExportResult{T}"/> containing the item.</returns>
    public static ExportResult<T> Success(T item) => new() { Item = item, IsSuccess = true };

    /// <summary>
    /// Creates a failed export result with error messages.
    /// </summary>
    /// <param name="messages">The error messages describing the failure.</param>
    /// <returns>A failed <see cref="ExportResult{T}"/> with the error messages.</returns>
    public static ExportResult<T> Failure(params string[] messages) => new() { Messages = messages, IsSuccess = false };

    /// <summary>
    /// Gets or initializes the error messages from a failed export.
    /// </summary>
    public IEnumerable<string> Messages { get; init; } = [];

    /// <summary>
    /// Gets or initializes the exported item, or default if the export failed.
    /// </summary>
    public T? Item { get; init; }

    /// <summary>
    /// Gets or initializes whether the export was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets a value indicating whether the export failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;
}
