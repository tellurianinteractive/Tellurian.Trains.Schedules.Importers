using System.Globalization;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

public sealed record OperationLocation : IEquatable<OperationLocation>
{
    public Layout Layout { get; internal set; } = default!;

    public int Id { get; init; }
    public string Name { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Signature { get; init; }
    public bool IsShadow { get; init; }
    public bool IsManned { get; init; } = true;
    public ICollection<StationTrack> Tracks { get; }
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
    public override int GetHashCode() => Signature.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Name;
    public static OperationLocation Example => new(1, "Ytterby", "Yb");
}

public static class StationExtensions
{
    public static IEnumerable<Train> Trains(this OperationLocation? me) =>
        me is null ? [] : me.Calls().Where(c => c.Train.HasValue()).Select(c => c.Train!).Distinct();

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
        stationTrack.Station = station.ValueOrException(nameof(station));
        station.Tracks.Add(stationTrack);
        return stationTrack;
    }
}
