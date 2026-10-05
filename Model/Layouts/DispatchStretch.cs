using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;
/// <summary>
/// A stretch between two dispatch endpoints: stations with a dispatcher, or locations remotely
/// dispatched from one (see <c>IsDispatchEndpoint</c>).
/// </summary>
public class DispatchStretch
{
    /// <summary>
    /// Contructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public DispatchStretch(int id, OperationLocation from, OperationLocation to)
    {
        Id = id;
        From = from;
        To = to;
        Stretches = [];
    }

    /// <summary>
    /// Creates a dispatch stretch spanning the given ordered track stretches. The end stations are
    /// derived from the first stretch's start and the last stretch's end.
    /// </summary>
    /// <param name="id">The unique identifier for this dispatch stretch.</param>
    /// <param name="stretches">The ordered, contiguous track stretches the dispatch stretch comprises.</param>
    public DispatchStretch(int id, IEnumerable<TrackStretch> stretches)
    {
        Id = id;
        Stretches = [.. stretches];
        From = Stretches.First().Start;
        To = Stretches.Last().End;
    }

    [JsonConstructor]
    private DispatchStretch()
    {
        From = default!;
        To = default!;
        Stretches = [];
    }

    /// <summary>
    /// Gets or sets the unique identifier for this dispatch stretch.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The dispatch endpoint in one end of the <see cref="DispatchStretch"/>
    /// </summary>
    public OperationLocation From { get; set; }

    /// <summary>
    /// The dispatch endpoint in the other end of the <see cref="DispatchStretch"/>
    /// </summary>
    public OperationLocation To { get; set; }

    /// <summary>
    /// The ordered, contiguous track stretches this dispatch stretch comprises. Empty for dispatch
    /// stretches loaded from older documents that did not record them, or created from endpoints only.
    /// </summary>
    public ICollection<TrackStretch> Stretches { get; set; }

    /// <summary>
    /// The operation locations passed through between <see cref="From"/> and <see cref="To"/> that are
    /// not themselves dispatch endpoints (typically unmanned locations).
    /// </summary>
    [JsonIgnore]
    public IEnumerable<OperationLocation> IntermediateLocations =>
        Stretches.Count == 0 ? [] : Stretches.Take(Stretches.Count - 1).Select(s => s.End);

    /// <summary>
    /// Prints the start and end stations for the dispatch stretch.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{From}-{To}";
}
