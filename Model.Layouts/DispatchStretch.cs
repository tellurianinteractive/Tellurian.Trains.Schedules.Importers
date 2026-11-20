using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>The line between two dispatch endpoints, of track stretches in order.</summary>
public sealed class DispatchStretch
{
    /// <summary>Creates a dispatch stretch between two locations, without track stretches.</summary>
    public DispatchStretch(int id, OperationLocation from, OperationLocation to)
    {
        Id = id;
        From = from;
        To = to;
        Stretches = [];
    }

    /// <summary>Creates a dispatch stretch over track stretches in order.</summary>
    public DispatchStretch(int id, IEnumerable<TrackStretch> stretches)
    {
        Id = id;
        Stretches = [.. stretches];
        From = Stretches.First().Start;
        To = Stretches.Last().End;
    }

    /// <summary>For JSON and EF Core.</summary>
    [JsonConstructor]
    private DispatchStretch()
    {
        From = default!;
        To = default!;
        Stretches = [];
    }

    /// <summary>The identity of the dispatch stretch within the layout.</summary>
    public int Id { get; set; }

    /// <summary>The endpoint at one end.</summary>
    public OperationLocation From { get; set; }

    /// <summary>The endpoint at the other end.</summary>
    public OperationLocation To { get; set; }

    /// <summary>The track stretches between the endpoints, in order.</summary>
    public ICollection<TrackStretch> Stretches { get; set; }

    /// <summary>The locations between the endpoints, in order.</summary>
    [JsonIgnore]
    public IEnumerable<OperationLocation> IntermediateLocations =>
        Stretches.Count == 0 ? [] : Stretches.Take(Stretches.Count - 1).Select(s => s.End);

    /// <inheritdoc/>
    public override string ToString() => $"{From}-{To}";
}
