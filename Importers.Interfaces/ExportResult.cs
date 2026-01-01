namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public readonly struct ExportResult<T>
{
    public ExportResult() { }

    public static ExportResult<T> Success(T item) => new() { Item = item, IsSuccess = true };
    public static ExportResult<T> Failure(params string[] messages) => new() { Messages = messages, IsSuccess = false };

    public IEnumerable<string> Messages { get; init; } = [];
    public T? Item { get; init; }
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
}
