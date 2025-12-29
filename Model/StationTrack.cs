namespace Tellurian.Trains.Schedules.Model;


public sealed record StationTrack : IEquatable<StationTrack>
{
    public StationTrack(int id, string number) : this(id, number, true, true) { }

    public StationTrack(int id, string number, bool isMain, bool isScheduled)
    {
        Id = id;
        Number = number;
        IsMain = isMain;
        IsScheduled = isScheduled;
        Calls = [];
    }

    public int Id { get; init; }
    public string Number { get; }
    public int DisplayOrder { get; init; }
    public bool IsScheduled { get; init; } = true;
    public bool IsMain { get; init; }
    public double Length { get; init; }
    public string Usage { get; init; } = string.Empty;

    public OperationLocation Station { get; set; } = default!;

    public ICollection<StationCall> Calls { get; }

    public bool Equals(StationTrack? other) => Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase) && Station.Equals(other?.Station);
    public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Number;
    public static StationTrack Example { get { return new StationTrack(1, "1") { Station = OperationLocation.Example }; } }
}

public static class StationTrackExtensions
{
    internal static StationCall Add(this StationTrack me, StationCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        if (!me.Calls.Contains(call))
        {
            me.Calls.Add(call);
        }
        return call;
    }
}
