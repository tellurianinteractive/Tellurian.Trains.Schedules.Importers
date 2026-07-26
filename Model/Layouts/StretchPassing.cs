using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Represents a train passing through a track stretch between two stations.
/// </summary>
public sealed record StretchPassing
{
    /// <summary>
    /// Gets the train making this passing.
    /// </summary>
    public Train Train { get; }

    /// <summary>
    /// Gets the departure station call.
    /// </summary>
    public StationCall From { get; }

    /// <summary>
    /// Gets the arrival station call.
    /// </summary>
    public StationCall To { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="StretchPassing"/> with the specified values.
    /// </summary>
    /// <param name="train">The train making the passing.</param>
    /// <param name="from">The departure station call.</param>
    /// <param name="to">The arrival station call.</param>
    public StretchPassing(Train train, StationCall from, StationCall to)
    {
        Train = train;
        From = from.ValueOrException(nameof(from));
        To = to.ValueOrException(nameof(to));
    }

    /// <summary>
    /// Gets the arrival time at the destination station.
    /// </summary>
    public Time Arrival => To.Arrival;

    /// <summary>
    /// Gets the departure time from the origin station.
    /// </summary>
    public Time Departure => From.Departure;

    /// <summary>
    /// Gets the stations and times of the passing without the train identity,
    /// for use in messages where the identity is shown separately.
    /// </summary>
    public string SpanText =>
        string.Format(CultureInfo.CurrentCulture, "{0} {1} - {2} {3}", From.OperationLocation.Name, Departure.HHMM(), To.OperationLocation.Name, Arrival.HHMM());

    /// <inheritdoc/>
    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, "{0} {1}: {2} - {3}: {4}", Train.Identity, From.OperationLocation.Name, Departure.HHMM(), To.OperationLocation.Name, Arrival.HHMM());
}
