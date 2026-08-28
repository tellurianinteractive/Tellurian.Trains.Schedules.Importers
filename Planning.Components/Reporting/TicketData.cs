using System.Globalization;
using Tellurian.Trains.Schedules.Planning.App.Translations;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// One printed return ticket: the journey out from <see cref="Origin"/> to <see cref="Destination"/>
/// and the journey home again.
/// </summary>
/// <remarks>
/// Only the two stations tell the halves of a ticket apart. The seller and the validity belong to the
/// ticket rather than to a direction — the whole ticket is bought once, at the origin, before the
/// passenger leaves — so both halves carry them unchanged.
/// </remarks>
public sealed record TicketData
{
    /// <summary>The location the ticket is sold at, and where the outward journey starts.</summary>
    public required string Origin { get; init; }

    /// <summary>The location the outward journey ends at.</summary>
    public required string Destination { get; init; }

    /// <summary>
    /// The company selling the ticket: the passenger operator with the most departures from
    /// <see cref="Origin"/>, or <c>null</c> when no passenger train departs there under a company at all.
    /// </summary>
    public Company? SoldBy { get; init; }

    /// <summary>First day of the meeting the ticket is valid for, or <c>null</c> when none is booked.</summary>
    public DateOnly? ValidFrom { get; init; }

    /// <summary>Last day of the meeting the ticket is valid for. See <see cref="ValidFrom"/>.</summary>
    public DateOnly? ValidTo { get; init; }
}

/// <summary>
/// Provides extension methods for <see cref="TicketData"/> and for projecting a timetable into tickets.
/// </summary>
public static class TicketDataExtensions
{
    extension(TicketData ticket)
    {
        /// <summary>
        /// The line printed at the foot of both halves, naming who sold the ticket and where. A location
        /// with no passenger operator still says where it was sold: the location always exists, the
        /// company need not.
        /// </summary>
        /// <param name="translator">Resolves a resource key to its text in the current language.</param>
        public string SoldByText(Translator translator) =>
            ticket.SoldBy is { } company
                ? string.Format(CultureInfo.CurrentCulture, translator("SoldByAt"), company.Signature, ticket.Origin)
                : string.Format(CultureInfo.CurrentCulture, translator("SoldAt"), ticket.Origin);

        /// <summary>
        /// How long the ticket is valid: the days of the meeting the layout is planned for. Empty when no
        /// meeting is booked, so the ticket omits the line rather than printing a placeholder date.
        /// </summary>
        /// <param name="translator">Resolves a resource key to its text in the current language.</param>
        public string Validity(Translator translator) =>
            ticket.ValidFrom is { } from && ticket.ValidTo is { } to
                ? string.Format(CultureInfo.CurrentCulture, translator("ValidBetween"),
                    from.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture),
                    to.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture))
                : "";
    }

    extension(Timetable timetable)
    {
        /// <summary>
        /// Projects the timetable into the return tickets that can be sold at the meeting: one from every
        /// location that exchanges passengers to every other such location.
        /// </summary>
        /// <remarks>
        /// A journey and its reverse are both produced, because they are not the same ticket: each is sold
        /// at its own origin, by that location's own operator. Tickets come out ordered by selling location
        /// and then by destination, so a printed run stays in blocks a station can be handed.
        /// </remarks>
        public IReadOnlyList<TicketData> ToPassengerTickets()
        {
            var locations = timetable.Layout.OperationLocations
                .Where(location => location.HasPassengerExchange)
                .OrderBy(location => location.Name, StringComparer.CurrentCulture)
                .ToList();
            // One location has nowhere to travel to, so there is nothing to sell.
            if (locations.Count < 2) return [];

            var sellers = SellingCompanies(timetable);
            var general = timetable.Layout.Settings.General;

            return
            [
                .. locations.SelectMany(origin => locations
                    .Where(destination => destination.Id != origin.Id)
                    .Select(destination => new TicketData
                    {
                        Origin = origin.Name,
                        Destination = destination.Name,
                        SoldBy = sellers.GetValueOrDefault(origin.Id),
                        ValidFrom = general.ValidFrom,
                        ValidTo = general.ValidTo,
                    }))
            ];
        }
    }

    /// <summary>
    /// The company selling at each location, keyed by <see cref="OperationLocation.Id"/>: the passenger
    /// operator that departs from there most often.
    /// </summary>
    /// <remarks>
    /// Only passenger trains are counted. A freight operator may well run the most trains through a
    /// location without ever carrying anyone, and it is a passenger ticket the location is selling.
    /// Counted in one pass over the trains rather than once per location, since every location asks the
    /// same question of the same calls.
    /// </remarks>
    private static Dictionary<int, Company> SellingCompanies(Timetable timetable) =>
        timetable.Trains
            .Where(train => train.IsPassenger && train.EffectiveCompany is not null)
            .SelectMany(train => train.DepartureCalls
                .Select(call => (LocationId: call.OperationLocation.Id, Company: train.EffectiveCompany!)))
            .GroupBy(departure => departure.LocationId)
            .ToDictionary(
                atLocation => atLocation.Key,
                atLocation => atLocation
                    .GroupBy(departure => departure.Company)
                    .OrderByDescending(byCompany => byCompany.Count())
                    // A tie is broken by signature, so the same plan always prints the same seller
                    // rather than whichever company the enumeration happened to reach first.
                    .ThenBy(byCompany => byCompany.Key.Signature, StringComparer.CurrentCulture)
                    .First().Key);
}
