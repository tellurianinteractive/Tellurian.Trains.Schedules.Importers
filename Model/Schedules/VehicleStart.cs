namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Where a rolling stock item is to stand before the meeting begins: at the start of the first train part it
/// works on the first session or day it is in operation.
/// </summary>
/// <param name="FirstPosition">The 1-based session number — or, for a layout counting in days, the position
/// in the operating week — on which the vehicle is first in operation.</param>
/// <param name="FirstPart">The first train part the vehicle works on that session or day.</param>
/// <param name="IsEverySession">Whether the vehicle works on every session or day of the operating period, so that
/// it is in operation throughout rather than joining or leaving part of the way through.</param>
public sealed record VehicleStart(int FirstPosition, ScheduledTrainPart FirstPart, bool IsEverySession)
{
    /// <summary>The call the vehicle starts from.</summary>
    public StationCall From => FirstPart.From;

    /// <summary>The location the vehicle starts at.</summary>
    public OperationLocation Location => From.OperationLocation;

    /// <summary>The track the vehicle is to stand on.</summary>
    public StationTrack Track => From.Track;

    /// <summary>The departure of the vehicle's first train part.</summary>
    public Time Departure => From.Departure;
}
