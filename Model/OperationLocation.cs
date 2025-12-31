using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

public sealed class OperationLocation : IEquatable<OperationLocation>
{
    // FK property for EF Core
    public int LayoutId { get; set; }
    public Layout Layout { get; set; } = default!;

    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Signature { get; set; }
    public bool IsShadow { get; set; }
    public bool IsManned { get; set; } = true;
    public ICollection<StationTrack> Tracks { get; set; }

    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private OperationLocation()
    {
        Name = string.Empty;
        Signature = string.Empty;
        Tracks = [];
    }

    public OperationLocation(int id, string name, string signature)
    {
        Id = id;
        name = name.TextOrException(nameof(name), string.Format(CultureInfo.CurrentCulture, Strings.NameOfObjectIsRequired, Strings.Station.ToLowerInvariant()));
        Name = name.Replace("_", " ", StringComparison.OrdinalIgnoreCase);
        Signature = signature.TextOrException(nameof(signature), string.Format(CultureInfo.CurrentCulture, Strings.SignatureOfStationIsRequired));
        Tracks = [];
    }

    public StationTrack this[string number] => Tracks.SingleOrDefault(t => t.Number == number) ?? throw new InvalidOperationException($"Station {Name} has no track '{number}'");
    public bool Equals(OperationLocation? other) => Signature.Equals(other?.Signature, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is OperationLocation other && Equals(other);
    public override int GetHashCode() => Signature.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Name;
    public static OperationLocation Example => new(1, "Ytterby", "Yb");
}

public static class StationExtensions
{
    public static IEnumerable<Train> Trains(this OperationLocation? me) =>
        me is null ? [] : me.Calls().Where(c => c.Train.HasValue).Select(c => c.Train!).Distinct();

    public static IEnumerable<StationCall> Calls(this OperationLocation me) =>
       me is null ? [] : me.Tracks.SelectMany(t => t.Calls);
    public static Maybe<StationTrack> Track(this OperationLocation? station, string number)
         => new(station?.Tracks.SingleOrDefault(t => t.Number == number),
             string.Format(CultureInfo.CurrentCulture, Strings.StationHasNotTrackNumber, station?.Name, number));

    public static bool HasTrack(this OperationLocation me, string number)
        => me?.Tracks.Any(t => t.Number == number) ?? false;

    public static StationTrack Add(this OperationLocation station, StationTrack stationTrack)
    {
        stationTrack = stationTrack.ValueOrException(nameof(stationTrack));
        ArgumentNullException.ThrowIfNull(stationTrack);
        station = station.ValueOrException(nameof(station));
        stationTrack.Station = station;
        stationTrack.StationId = station.Id;
        station.Tracks.Add(stationTrack);
        return stationTrack;
    }
}
