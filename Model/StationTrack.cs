namespace Tellurian.Trains.Schedules.Model;


public sealed class StationTrack : IEquatable<StationTrack>
{
    // Private parameterless constructor for EF Core
    private StationTrack()
    {
        Number = string.Empty;
        Calls = [];
    }

    public StationTrack(int id, string number) : this(id, number, true, true) { }

    public StationTrack(int id, string number, bool isMain, bool isScheduled)
    {
        Id = id;
        Number = number;
        IsMain = isMain;
        IsScheduled = isScheduled;
        Calls = [];
    }

    public int Id { get; set; }
    public string Number { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsScheduled { get; set; } = true;
    public bool IsMain { get; set; }
    public double Length { get; set; }
    public string Usage { get; set; } = string.Empty;

    // FK property for EF Core
    public int StationId { get; set; }
    public OperationLocation Station { get; set; } = default!;

    public ICollection<StationCall> Calls { get; set; }

    public bool Equals(StationTrack? other) => Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase) && Station.Equals(other?.Station);
    public override bool Equals(object? obj) => obj is StationTrack other && Equals(other);
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
