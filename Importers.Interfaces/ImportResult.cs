using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public readonly struct ImportResult<T>
{
    public static ImportResult<T> Success() => new([], [], true);
    public static ImportResult<T> Success(T item) => new([item], [], true);
    public static ImportResult<T> Success(T item, Message message) => new([item], [message], true);
    public static ImportResult<T> Success(T item, IEnumerable<Message> messages) => new([item], messages, true);
    public static ImportResult<T> Success(IEnumerable<T> items) => new(items, [], true);
    public static ImportResult<T> Success(IEnumerable<T> items, IEnumerable<Message> messages) => new(items, messages, true);
    public static ImportResult<T> Success(IEnumerable<T> items, Message message) => new(items, [message], true);
    public static ImportResult<T> Failure(Message message) => new([], [message], false);
    public static ImportResult<T> Failure(IEnumerable<Message> messages) => new([], messages, false);
    public static ImportResult<T> SuccessIfNoErrorMessagesOtherwiseFailure(T? item, IEnumerable<Message> messages) => new(item is null ? [] : new[] { item }, messages, !messages.Any(m => m.Severity > Severity.Warning));

    [JsonConstructor]
    public ImportResult()
    {
        Items = [];
        Messages = [];
    }
    public ImportResult(IEnumerable<T> items, IEnumerable<Message> messages, bool isSuccess)
    {
        Items = items;
        Messages = [.. messages];
        IsSuccess = isSuccess;
    }
    public string? Name { get; init; }
    [JsonIgnore]
    public IEnumerable<T> Items { get; init; }
    public Message[] Messages { get; init; }
    public bool IsSuccess { get; init; }
    [JsonIgnore] public bool IsFailure => !IsSuccess;
    [JsonIgnore] public T Item => Items.First();
}

public static class ImportResultExtensions
{
    static readonly JsonSerializerOptions options = new() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
    public static string Json<T>(this ImportResult<T> me)
    {
        return JsonSerializer.Serialize(me, options);
    }

    public static void Write<T>(this ImportResult<T> me)
    {
        File.WriteAllText(Path.Combine(Path.GetTempPath(), $"{me.Item}.json"), me.Json());
    }
}