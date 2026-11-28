namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Derives which stations are passenger interchanges: the places a passenger travelling on a ticket
/// leaves one train and goes on by the next.
/// </summary>
/// <remarks>
/// <para>
/// Two things have to hold at once, which is why this is derived rather than read straight off the
/// station. The meeting has to be worked with passenger tickets at all
/// (<see cref="Settings.GeneralSettings.UsePassengerTickets"/>) — with no tickets in use there is
/// nobody to hand over — and the station has to be one of the places marked for it
/// (<see cref="Station.IsPassengerInterchange"/>).
/// </para>
/// <para>
/// A meeting that drops the tickets keeps its stations marked, and nothing is derived from the marks
/// meanwhile: the same arrangement as a lock key left out of force, so taking the tickets up again
/// costs nothing but the setting.
/// </para>
/// </remarks>
public static class PassengerInterchangeExtensions
{
    extension(Timetable timetable)
    {
        /// <summary>
        /// The layout's passenger interchanges, in layout order. Empty where the meeting is not worked
        /// with passenger tickets, so a caller never has to ask the setting itself.
        /// </summary>
        public IReadOnlyList<Station> PassengerInterchanges =>
            !timetable.Settings.General.UsePassengerTickets ? [] :
            [
                .. timetable.Layout.OperationLocations
                    .OfType<Station>()
                    .Where(station => station.HasPassengerExchange && timetable.PlanningOf(station).IsPassengerInterchange)
            ];

        /// <summary>
        /// Whether the passengers going on by another train are handed over at <paramref name="location"/>: the station is
        /// marked a passenger interchange and the timetable is worked with passenger tickets. What every report and note
        /// asks, so neither has to combine the two itself.
        /// </summary>
        public bool ExchangesTransferringPassengers(OperationLocation location) =>
            location is Station { HasPassengerExchange: true } station &&
            timetable.Settings.General.UsePassengerTickets &&
            timetable.PlanningOf(station).IsPassengerInterchange;
    }

    extension(OperationLocation location)
    {
        /// <summary>
        /// Whether this location may be marked a passenger interchange: a station, and one that exchanges
        /// passengers. Somewhere a passenger can neither board nor alight is not somewhere they can
        /// change trains.
        /// </summary>
        public bool CanBePassengerInterchange =>
            location is Station && location.HasPassengerExchange;
    }
}
