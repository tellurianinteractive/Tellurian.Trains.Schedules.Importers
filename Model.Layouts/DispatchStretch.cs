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

    /// <summary>The endpoint at one end. Written by its id, <see cref="FromId"/>.</summary>
    [JsonIgnore]
    public OperationLocation From
    {
        get;
        set { field = value; _fromId = null; }
    }

    /// <summary>The id of <see cref="From"/>, the form it is written in; the id read until the location is found.</summary>
    public int? FromId
    {
        get => From?.Id ?? _fromId;
        set => _fromId = value;
    }
    private int? _fromId;

    /// <summary>The endpoint at the other end. Written by its id, <see cref="ToId"/>.</summary>
    [JsonIgnore]
    public OperationLocation To
    {
        get;
        set { field = value; _toId = null; }
    }

    /// <summary>The id of <see cref="To"/>, the form it is written in; the id read until the location is found.</summary>
    public int? ToId
    {
        get => To?.Id ?? _toId;
        set => _toId = value;
    }
    private int? _toId;

    /// <summary>The track stretches between the endpoints, in order. Written by their ids, <see cref="StretchIds"/>.</summary>
    [JsonIgnore]
    public ICollection<TrackStretch> Stretches
    {
        get;
        set { field = value; _stretchIds = null; }
    }

    /// <summary>
    /// The ids of <see cref="Stretches"/>, in order, the form they are written in; the ids read until the stretches are
    /// found. Null, and so not written, when there are none.
    /// </summary>
    public IList<int>? StretchIds
    {
        get => Stretches is { Count: > 0 } ? [.. Stretches.Select(s => s.Id)] : _stretchIds;
        set => _stretchIds = value;
    }
    private IList<int>? _stretchIds;

    /// <summary>The locations between the endpoints, in order.</summary>
    [JsonIgnore]
    public IEnumerable<OperationLocation> IntermediateLocations =>
        Stretches.Count == 0 ? [] : Stretches.Take(Stretches.Count - 1).Select(s => s.End);

    /// <inheritdoc/>
    public override string ToString() => $"{From}-{To}";
}
