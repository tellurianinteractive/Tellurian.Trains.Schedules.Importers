
using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Utilities;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

public record TurnusData
{
    /// <summary>The vehicle's unique id, used as a last-resort card identity when it has neither a turnus number nor an external id.</summary>
    public int Id { get; init; }
    public ScheduledObjectType ScheduledObjectType { get; init; }
    public string Company { get; init; } = string.Empty;
    public string Class { get; init; } = string.Empty;
    public int Number { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public Sessions Sessions { get; init; }
    public string Remark { get; init; } = string.Empty;
    public IEnumerable<TrainPart> TrainParts { get; init; } = [];
    public bool TurnForNextSession { get; init; } = false;

    /// <summary>The first weekday of the operating week, used to name the operating days on the card.</summary>
    public DayOfWeek StartDay { get; init; } = DayOfWeek.Monday;

    /// <summary>Whether the operating period is shown as weekdays (true) or session numbers (false).</summary>
    public bool UseDays { get; init; }

    /// <summary>The number of sessions/days in the operating period, used to tell a whole-period card from a partial one.</summary>
    public int MaxSessions { get; init; } = 14;

}

public static class TurnusDataExtensions
{
    extension(TurnusData data)
    {
        public string Key => data.ExternalId.HasValue ? $"{data.ExternalId}-{data.ScheduledObjectType}" : $"{data.Company}-{data.Class}-{data.Number}-{data.ScheduledObjectType}";
        public string Display => data.ExternalId.HasValue ? data.ExternalId : data.Number > 0 ? $"{data.Company} {data.Number:D2}" : $"{data.Company}";
        public bool HasClass => data.Class.HasValue && data.ExternalId.IsEmpty;

        public string ClassOrContentLabel(Translator translator) =>
            data.ScheduledObjectType == ScheduledObjectType.Cargo ? translator("Content") : translator("Class");
        public string CrossLineColor => data.ScheduledObjectType switch
        {
            ScheduledObjectType.Locomotive or ScheduledObjectType.Trainset => "#ffc0cb",
            ScheduledObjectType.Wagonset => "#66ff99",
            ScheduledObjectType.CargoFlow or ScheduledObjectType.Cargo => "#ffff99",
            _ => "#cccccc"
        };

        /// <summary>
        /// Full card identity: <see cref="Key"/> plus the operating sessions. The sessions are always part of
        /// the identity — a vehicle that works several distinct session/day combinations (e.g. a daily working
        /// on Sat-Sun and a fuller working on Mo-Fr) yields one card per combination, and they must not merge
        /// even when there is no turnus number (<see cref="TurnusData.Number"/> 0).
        /// </summary>
        public string KeyWithSession => $"{data.Key}.{data.Sessions}";

        public string ObjectTypeName(Translator translator) => translator(data.ScheduledObjectType.ToString());

        /// <summary>The label for the operating-period line — "Days" for a day layout, else "Sessions".</summary>
        public string OperatingPeriodLabel(Translator translator) => translator(data.UseDays ? "Days" : "Sessions");

        /// <summary>
        /// The sessions/days qualifying this card, as short weekday names (e.g. <c>Mo-Fr</c>) or session numbers
        /// (e.g. <c>1-5</c>) per the layout. Empty when the card covers the whole operating period, so nothing
        /// needs qualifying.
        /// </summary>
        public string OperatingPeriod(Translator translator)
        {
            if (data.Sessions.CoversAllWithin(data.UseDays, data.MaxSessions)) return string.Empty;
            if (!data.UseDays) return data.Sessions.SessionsNumbers;
            var key = data.Sessions.ShortDayNamesResourceKey(data.StartDay);
            var keys = key.Split('-', ',', StringSplitOptions.TrimEntries);
            if (keys.Length > 1)
                return key.Contains(',')
                    ? string.Join(",", keys.Select(rk => translator(rk)))
                    : string.Join("-", keys.Select(rk => translator(rk)));
            return translator(key);
        }
    }

    extension(IEnumerable<ScheduledObject> items)
    {
        /// <summary>
        /// Projects the vehicles into turnus cards. A vehicle yields one card per unique session/day
        /// combination it works (see <see cref="ScheduledObjectExtensions.SessionCombinations"/>): trains that
        /// run beyond one another within a schedule, or assignments that give a vehicle different sessions to
        /// different schedules, each produce their own card. A card whose turnus number also appears on another
        /// session set is flagged as turning over.
        /// </summary>
        /// <param name="useDays">Whether the operating period is measured in weekdays rather than session numbers.</param>
        /// <param name="maxSessions">The number of sessions/days in the operating period (1-14).</param>
        /// <param name="startDay">The first weekday of the operating week, used to name the operating days.</param>
        public IEnumerable<TurnusData> ToTurnusData(bool useDays, int maxSessions, DayOfWeek startDay)
        {
            // One row per (vehicle, session/day combination); each row carries the parts worked on it.
            var rows = items.Where(i => i.HasTurnusCard)
                .SelectMany(scheduledObject => scheduledObject.SessionCombinations(useDays, maxSessions)
                    .Select(combination => new TurnusData
                    {
                        Id = scheduledObject.Id,
                        ScheduledObjectType = scheduledObject.ObjectType,
                        Company = scheduledObject.Company?.Signature ?? string.Empty,
                        Class = scheduledObject.Class,
                        // The turnus number is carried by the assignment that covers these sessions; a
                        // combination is drawn from a single assignment in practice.
                        Number = scheduledObject.Number,
                        ExternalId = scheduledObject.ExternalId ?? string.Empty,
                        Sessions = combination.Sessions,
                        StartDay = startDay,
                        UseDays = useDays,
                        MaxSessions = maxSessions,
                        // Suppress a remark that only repeats the turnus (the external id), e.g. an
                        // imported wagonset whose remark is a copy of its identifier.
                        Remark = scheduledObject.Remark.EqualsCaseInsensitive(scheduledObject.ExternalId)
                            ? string.Empty
                            : scheduledObject.Remark ?? string.Empty,
                        TrainParts = combination.Parts,
                    }));

            // One card per full identity (KeyWithSession); merge any rows that share it (e.g. several
            // vehicles forming one turnus).
            var cards = rows
                .GroupBy(row => row.KeyWithSession)
                .Select(group => group.First() with
                {
                    TrainParts = [.. group
                            .SelectMany(row => row.TrainParts)
                            .Distinct()
                            .OrderBy(part => part.Departure?.Value)],
                })
                .ToList();

            // A turnus (Key, ignoring sessions) running on more than one session set turns over.
            var sessionVariants = cards
                .GroupBy(card => card.Key)
                .ToDictionary(group => group.Key, group => group.Count());

            return [.. cards
                    .Select(card => card with { TurnForNextSession = sessionVariants[card.Key] > 1 })
                    .OrderBy(card => card.KeyWithSession)];
        }
    }
}
