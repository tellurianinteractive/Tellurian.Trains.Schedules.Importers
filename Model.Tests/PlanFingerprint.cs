using System.Globalization;
using System.Text;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// What a plan holds, and above all what refers to what, as one text per part of the plan, so that two readings of a
/// plan can be compared. A train part, a cargo flow too, is known by its calls and its schedule, since its own id may be
/// given anew.
/// </summary>
internal static class PlanFingerprint
{
    public static IReadOnlyDictionary<string, string> Of(Plan plan)
    {
        var layout = plan.Layout;
        var timetable = plan.Timetable;
        var result = new Dictionary<string, string>
        {
            ["locations"] = Join(layout.OperationLocations.OrderBy(l => l.Id), l =>
                $"{l.Id}:{l.GetType().Name}:{l.Name}:{l.Signature}:{l.CountryId}:{l.CargoServedFrom?.Id}:{l.ControlledBy?.Id}:{l.LockKey?.Name}@{l.LockKey?.HeldAt?.Id}:" +
                $"{l.HasPassengerExchange}:{l.HasCargoExchange}:{l.IsChangingTrainDirectionPossible}:" +
                (l is Station s ? $"{s.IsShadow}:{s.IsManned}:{s.HasTurntable}:{string.Join('+', s.Regions.Select(r => r.Id))}" : "") +
                (l is SignalControlledLocation c ? $"{c.TrainsCanCross}" : "")),
            ["tracks"] = Join(layout.OperationLocations.SelectMany(l => l.Tracks).OrderBy(t => t.Id), t =>
                $"{t.Id}:{t.Station.Id}:{t.Number}:{t.DisplayOrder}:{t.IsScheduled}:{t.IsMain}:{t.IsStub}:{t.Length}:{t.PlatformLength}:{t.IsLockable}:{t.Usage}:{t.Calls.Count}"),
            ["location planning"] = Join(timetable.LocationPlanning.OrderBy(p => p.LocationId), p =>
                $"{p.LocationId}:{p.HideMeets}:{p.HidePassings}:{p.Instructions.Length}:{p.PhoneNumber}:{p.Timings.MinimumStopMinutes}/{p.Timings.LocoRunaroundRealMinutes}/{p.Timings.TrainClearanceRealMinutes}:" +
                $"{p.StartLabels}:{p.ShiftCount}:{string.Join('+', p.StaffPosts.Select(post => post.Roles))}:{p.SplitDispatchByDirection}:{p.IsPassengerInterchange}"),
            ["track routes"] = Join(timetable.StationTrackRoutes.OrderBy(r => r.TrackId), r => $"{r.TrackId}:{r.PreviousLocationId}:{r.NextLocationId}:{r.AppliesInBothDirections}"),
            ["stretches"] = Join(layout.TrackStretches.OrderBy(s => s.Id), s => $"{s.Id}:{s.Start.Id}-{s.End.Id}:{s.Distance}:{s.TracksCount}:{s.Speed}:{s.Time}:{s.IsElectrified}"),
            ["timetable stretches"] = Join(timetable.TimetableStretches.OrderBy(s => s.Id), s => $"{s.Id}:{s.Number}:{s.Description}:{s.Color}:{string.Join('+', s.Stretches.Select(t => t.Id))}"),
            ["dispatch stretches"] = Join(timetable.DispatchStretches.OrderBy(s => s.Id), s => $"{s.Id}:{s.From?.Id}-{s.To?.Id}:{string.Join('+', s.Stretches.Select(t => t.Id))}"),
            ["topology"] = Join(timetable.TopologyPositions.OrderBy(p => p.LocationId), p => $"{p.LocationId}:{p.X}:{p.Y}"),
            ["countries"] = string.Join('|', layout.Countries.Select(c => c.Id)),
            ["companies"] = Join(timetable.Companies.OrderBy(c => c.Id), c => $"{c.Id}:{c.Name}:{c.Signature}:{c.CountryId}:{c.Logo?.Length}"),
            ["regions"] = Join(layout.Regions.OrderBy(r => r.Id), r => $"{r.Id}:{r.Name}:{r.CountryId}:{r.BackgroundColor}"),
            ["settings"] = $"{timetable.Settings.Identity.Theme}:{timetable.Settings.General.StartDay}:{timetable.Settings.GraphicTimetable.Orientation}:{timetable.Settings.TimeAndSpeed.FastClockSpeed}:{timetable.Settings.Validation.ValidateSchedules}",
            ["categories"] = Join(timetable.TrainCategories.OrderBy(c => c.Id), c => $"{c.Id}:{c.Prefix}:{c.Name}:{c.Content}:{c.IsShunting}:{c.StartLabels}:{c.Company?.Id}:{string.Join('+', c.StopLocationIds)}"),
            ["trains"] = Join(timetable.Trains.OrderBy(t => t.Id), t =>
                $"{t.Id}:{t.Number}:{t.Category?.Id}:{t.Company?.Id}:{t.Sessions}:{t.MaxSpeed}:{t.IsReversible}:{t.Length.Axles}/{t.Length.Wagons}/{t.Length.Meters}:" +
                string.Join(',', t.Calls.Select(c => $"{c.Id}@{c.Track.Id}:{c.Arrival}-{c.Departure}:{c.IsArrival}{c.IsDeparture}{c.TurnLoco}{c.Runaround}{c.PushWagons}:{c.Notes.Count}"))),
            ["cargo flows"] = Join(timetable.Trains.SelectMany(t => t.CargoFlows).Select(p => $"{p.From.Id}-{p.To.Id}:{p.CargoFlowOptions?.Id}:{p.PositionInTrain}").Order(StringComparer.Ordinal), p => p),
            ["cargo flow options"] = Join(timetable.CargoFlowOptions.OrderBy(o => o.Id), o =>
                $"{o.Id}:{string.Join('+', o.Origins.Select(x => x.Location.Id))}>{string.Join('+', o.Destinations.Select(x => $"{x.Location.Id}/{x.PositionInTrain}"))}:{o.ToAllDestinations}"),
            ["vehicles"] = Join(plan.ScheduledObjects.OrderBy(v => v.Id), v =>
                $"{v.Id}:{v.Number}:{v.ObjectType}:{v.TractionType}:{v.WagonSetKind}:{v.Company?.Id}:{v.StationedAtId}:{v.Units.Count}:" +
                string.Join(',', v.ScheduleAssignments.Select(a => $"{a.Id}>{a.Schedule.Id}:{a.Role}:{a.Sessions}"))),
            ["schedules"] = Join(plan.Schedules.OrderBy(s => s.Id), s => $"{s.Id}:{s.Number}:{string.Join(',', s.Parts.Select(Part))}"),
            ["duties"] = Join(plan.DriverDuties.OrderBy(d => d.Id), d => $"{d.Id}:{d.Identity}:{d.Sessions}:{d.Difficulty}:{d.Company?.Id}:{d.Notes.Count}:{string.Join(',', d.Parts.Select(Part))}"),
            ["participants"] = Join(plan.Participants.OrderBy(p => p.Id), p => $"{p.Id}:{p.Name}"),
            ["contributions"] = Join(plan.VehicleContributions.OrderBy(c => c.ScheduledObjectId), c => $"{c.ScheduledObjectId}:{c.Contributors.Count}"),
            ["other vehicles"] = Join(plan.OtherVehicles.OrderBy(v => v.Id), v => $"{v.Id}:{v.Type}:{v.ParticipantId}"),
        };
        return result;
    }

    private static string Part(ScheduledTrainPart part) =>
        $"{part.Schedule?.Id}/{part.From.Id}-{part.To.Id}/{part.FromTrack?.Id}-{part.ToTrack?.Id}/{part.TractionOptions?.Runaround}{part.TractionOptions?.FromLayover}{part.WagonSetOptions?.OrderInTrain}";

    private static string Join<T>(IEnumerable<T> items, Func<T, string> text)
    {
        var builder = new StringBuilder();
        foreach (var item in items) builder.Append(CultureInfo.InvariantCulture, $"{text(item)}\n");
        return builder.ToString();
    }

    /// <summary>Fails with the first part of the plan that differs, and the first line of it that does.</summary>
    public static void AssertSame(IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual)
    {
        foreach (var (part, text) in expected)
        {
            var other = actual[part];
            if (text == other) continue;
            var lines = text.Split('\n');
            var otherLines = other.Split('\n');
            var index = Enumerable.Range(0, Math.Max(lines.Length, otherLines.Length)).First(i => i >= lines.Length || i >= otherLines.Length || lines[i] != otherLines[i]);
            Assert.Fail($"{part} differ at line {index + 1}:\nexpected: {lines.ElementAtOrDefault(index)}\nactual:   {otherLines.ElementAtOrDefault(index)}");
        }
    }
}
