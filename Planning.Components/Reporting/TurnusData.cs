
using Tellurian.Localization;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.App.Translations;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

public record TurnusData
{
    public ScheduledObjectType ScheduledObjectType { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string Class { get; init; } = string.Empty;
    public int Number { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public Sessions Sessions { get; init; }
    public string Remark { get; init; } = string.Empty;
    public IEnumerable<TrainPart> TrainParts { get; init; } = [];
    public bool TurnForNextSession { get; init; } = false;

    /// <summary>The first weekday of the operating week, used to name the operating days on the card.</summary>
    public DayOfWeek StartDay { get; init; } = DayOfWeek.Monday;

    /// <summary>The turnus number, or the vehicle's external id when the number is 0 (no turnus number in XPLN).</summary>
    public string TurnusId => Number > 0 ? Number.ToString() : ExternalId;
}

public static class TurnusDataExtensions
{
    extension(TurnusData data)
    {
        public string ClassAndNumber =>
            data.Number > 0 ? $"{data.Class} {data.Number}" : string.Empty;

        public bool HasClass => data.Class.HasValue && data.ExternalId.IsEmpty;

        public string ClassOrContentLabel(Translator translator) =>
            data.ScheduledObjectType == ScheduledObjectType.Cargo ? translator("Content") : translator("Class");

        public string CompanyNameOrEmpty =>
            data.ExternalId.HasValue ? string.Empty : data.CompanyName;

        public string CrossLineColor => data.ScheduledObjectType switch
        {
            ScheduledObjectType.Locomotive or ScheduledObjectType.Trainset => "#ffc0cb",
            ScheduledObjectType.Wagonset => "#66ff99",
            ScheduledObjectType.CargoFlow or ScheduledObjectType.Cargo => "#ffff99",
            _ => "#cccccc"
        };

        /// <summary>Turnus identity without sessions: external id for XPLN, else company.number.</summary>
        public string Key => data.Number == 0 ? data.ExternalId : $"{data.CompanyName}.{data.Number:D3}";

        /// <summary>Full card identity: Key plus the operating sessions.</summary>
        public string KeyWithSession => data.Number == 0 ? data.ExternalId : $"{data.CompanyName}.{data.Number:D3}.{data.Sessions}";

        public string ObjectTypeName(Translator translator) => translator(data.ScheduledObjectType.ToString());

        public string OperatingSessions(Translator translator)
        {
            var key = data.Sessions.ShortDayNamesResourceKey(data.StartDay);
            var keys = key.Split('-', ',', StringSplitOptions.TrimEntries);
            if (keys.Length > 1)
            {
                if (key.Contains(','))
                    return string.Join(",", keys.Select(rk => translator(rk)));
                return string.Join("-", keys.Select(rk => translator(rk)));

            }
            else if (keys.Length == 1 && !key.StartsWith("Daily"))
            {
                return translator(key);
            }
            return string.Empty;
        }
    }

    extension(IEnumerable<ScheduledObject> items)
    {
        public IEnumerable<TurnusData> ToTurnusData(DayOfWeek startDay)
        {
                // One row per (vehicle, assignment); each row carries that assignment's train parts.
                var rows = items.Where(i => i.HasTurnusCard)
                    .SelectMany(scheduledObject => scheduledObject.ScheduleAssignments.Select(assignment => new
                    {
                        Data = new TurnusData
                        {
                            ScheduledObjectType = scheduledObject.ObjectType,
                            CompanyName = scheduledObject.Company?.DisplayName ?? string.Empty,
                            Class = scheduledObject.Class,
                            Number = assignment.Number,
                            ExternalId = scheduledObject.ExternalId ?? string.Empty,
                            Sessions = assignment.Sessions,
                            StartDay = startDay,
                            // Suppress a remark that only repeats the turnus (the external id), e.g. an
                            // imported wagonset whose remark is a copy of its identifier.
                            Remark = string.Equals(scheduledObject.Remark, scheduledObject.ExternalId, StringComparison.Ordinal)
                                ? string.Empty
                                : scheduledObject.Remark ?? string.Empty,
                        },
                        Parts = assignment.Schedule?.Parts ?? Enumerable.Empty<ScheduledTrainPart>(),
                    }));

                // One card per full identity (KeyWithSession); merge the rows' train parts.
                var cards = rows
                    .GroupBy(row => row.Data.KeyWithSession)
                    .Select(group => group.First().Data with
                    {
                        TrainParts = [.. group
                            .SelectMany(row => row.Parts)
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
