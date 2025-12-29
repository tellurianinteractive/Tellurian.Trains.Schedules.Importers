using System.Globalization;

namespace Tellurian.Trains.Schedules.Model;

public sealed record StretchPassing
{
    public Train Train { get; }
    public StationCall From { get; }
    public StationCall To { get; }

    public StretchPassing(Train train, StationCall from, StationCall to)
    {
        Train = train;
        From = from.ValueOrException(nameof(from));
        To = to.ValueOrException(nameof(to));
    }

    public Time Arrival => To.Arrival;
    public Time Departure => From.Departure;

    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, "{0} {1}: {2} - {3}: {4}", Train.Identity, From.Station.Name, Departure.HHMM(), To.Station.Name, Arrival.HHMM());
}

