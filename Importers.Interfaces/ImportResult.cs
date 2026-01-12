using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Represents the result of an import operation, containing imported items and validation messages.
/// </summary>
/// <typeparam name="T">The type of items being imported.</typeparam>
public readonly struct ImportResult<T>
{
    /// <summary>
    /// Creates a successful result with no items or messages.
    /// </summary>
    /// <returns>A successful <see cref="ImportResult{T}"/> with empty items.</returns>
    public static ImportResult<T> Success() => new([], [], true);

    /// <summary>
    /// Creates a successful result with a single item.
    /// </summary>
    /// <param name="item">The imported item.</param>
    /// <returns>A successful <see cref="ImportResult{T}"/> containing the item.</returns>
    public static ImportResult<T> Success(T item) => new([item], [], true);

    /// <summary>
    /// Creates a successful result with a single item and message.
    /// </summary>
    /// <param name="item">The imported item.</param>
    /// <param name="message">A message related to the import.</param>
    /// <returns>A successful <see cref="ImportResult{T}"/> containing the item and message.</returns>
    public static ImportResult<T> Success(T item, Message message) => new([item], [message], true);

    /// <summary>
    /// Creates a successful result with a single item and multiple messages.
    /// </summary>
    /// <param name="item">The imported item.</param>
    /// <param name="messages">Messages related to the import.</param>
    /// <returns>A successful <see cref="ImportResult{T}"/> containing the item and messages.</returns>
    public static ImportResult<T> Success(T item, IEnumerable<Message> messages) => new([item], messages, true);

    /// <summary>
    /// Creates a successful result with multiple items.
    /// </summary>
    /// <param name="items">The imported items.</param>
    /// <returns>A successful <see cref="ImportResult{T}"/> containing the items.</returns>
    public static ImportResult<T> Success(IEnumerable<T> items) => new(items, [], true);

    /// <summary>
    /// Creates a successful result with multiple items and messages.
    /// </summary>
    /// <param name="items">The imported items.</param>
    /// <param name="messages">Messages related to the import.</param>
    /// <returns>A successful <see cref="ImportResult{T}"/> containing the items and messages.</returns>
    public static ImportResult<T> Success(IEnumerable<T> items, IEnumerable<Message> messages) => new(items, messages, true);

    /// <summary>
    /// Creates a successful result with multiple items and a single message.
    /// </summary>
    /// <param name="items">The imported items.</param>
    /// <param name="message">A message related to the import.</param>
    /// <returns>A successful <see cref="ImportResult{T}"/> containing the items and message.</returns>
    public static ImportResult<T> Success(IEnumerable<T> items, Message message) => new(items, [message], true);

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <returns>A failed <see cref="ImportResult{T}"/> with the error message.</returns>
    public static ImportResult<T> Failure(Message message) => new([], [message], false);

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    /// <param name="messages">The error messages describing the failures.</param>
    /// <returns>A failed <see cref="ImportResult{T}"/> with the error messages.</returns>
    public static ImportResult<T> Failure(IEnumerable<Message> messages) => new([], messages, false);

    /// <summary>
    /// Creates a result that is successful if no error messages exist, otherwise creates a failure.
    /// </summary>
    /// <param name="item">The imported item, or null if import failed.</param>
    /// <param name="messages">Messages from the import operation.</param>
    /// <returns>A successful result if no messages have severity greater than Warning; otherwise a failure.</returns>
    public static ImportResult<T> SuccessIfNoErrorMessagesOtherwiseFailure(T? item, IEnumerable<Message> messages) => new(item is null ? [] : new[] { item }, messages, !messages.Any(m => m.Severity > Severity.Warning));

    /// <summary>
    /// Initializes a new instance of <see cref="ImportResult{T}"/> with default values.
    /// </summary>
    [JsonConstructor]
    public ImportResult()
    {
        Items = [];
        Messages = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ImportResult{T}"/> with the specified values.
    /// </summary>
    /// <param name="items">The imported items.</param>
    /// <param name="messages">Messages from the import operation.</param>
    /// <param name="isSuccess">Whether the import was successful.</param>
    public ImportResult(IEnumerable<T> items, IEnumerable<Message> messages, bool isSuccess)
    {
        Items = items;
        Messages = [.. messages];
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Gets or initializes the name associated with this import result.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the imported items.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<T> Items { get; init; }

    /// <summary>
    /// Gets or initializes the messages generated during import.
    /// </summary>
    public Message[] Messages { get; init; }

    /// <summary>
    /// Gets or initializes whether the import was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets a value indicating whether the import failed.
    /// </summary>
    [JsonIgnore] public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the first imported item.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Items"/> is empty.</exception>
    [JsonIgnore] public T Item => Items.First();
}

/// <summary>
/// Provides extension methods for <see cref="ImportResult{T}"/>.
/// </summary>
public static class ImportResultExtensions
{
    static readonly JsonSerializerOptions options = new() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };

    /// <summary>
    /// Serializes the import result to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of items in the result.</typeparam>
    /// <param name="me">The import result to serialize.</param>
    /// <returns>A JSON string representation of the import result.</returns>
    public static string Json<T>(this ImportResult<T> me)
    {
        return JsonSerializer.Serialize(me, options);
    }

    /// <summary>
    /// Writes the import result to a JSON file in the system's temp directory.
    /// </summary>
    /// <typeparam name="T">The type of items in the result.</typeparam>
    /// <param name="me">The import result to write.</param>
    public static void Write<T>(this ImportResult<T> me)
    {
        File.WriteAllText(Path.Combine(Path.GetTempPath(), $"{me.Item}.json"), me.Json());
    }
}
