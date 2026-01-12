using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;
/// <summary>
/// A stretch between two stations with dispatchers.
/// </summary>
public class DispatchStretch
{
    /// <summary>
    /// Contructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public DispatchStretch(int id, Station from, Station to)
    {
        Id = id;
        From = from;
        To = to;
    }

    [JsonConstructor]
    private DispatchStretch()
    {
        From = default!;
        To = default!;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this dispatch stretch.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The station in one end of the <see cref="DispatchStretch"/>
    /// </summary>
    public Station From { get; set; }

    /// <summary>
    /// The station in other end of the <see cref="DispatchStretch"/>
    /// </summary>
    public Station To { get; set; }
    /// <summary>
    /// Prints the start and end stations for the dispatch stretch.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{From}-{To}";
}
