using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Duties;

/// <summary>
/// Interactive building blocks for the Duties editor: creating <see cref="DriverDuty">duties</see> and
/// finding the train parts that can extend a duty.
/// </summary>
/// <remarks>
/// A duty is a sequence of the <see cref="ScheduledTrainPart">train parts</see> already defined in the
/// vehicle <see cref="Schedule">schedules</see> — the segments a driver actually drives, so only traction
/// parts (parts worked by a locomotive or trainset) are offered. A part stays owned by its schedule (which
/// resolves its traction unit) and may be referenced by several duties, but only one per session. Parts are
/// appended through the guarded <see cref="DriverDutyExtensions.Append"/>; the picker is pre-filtered by
/// <see cref="CandidatePartsFor"/> so it lists only parts that can extend the duty.
/// </remarks>
public static class DriverDutyEditingExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Creates a new empty <see cref="DriverDuty"/>, adds it to the plan and returns it. Its
        /// <see cref="DriverDuty.Identity"/> defaults to its id and its <see cref="DriverDuty.Sessions"/>
        /// to all sessions.
        /// </summary>
        public DriverDuty CreateDriverDuty()
        {
            plan = plan.ValueOrException(nameof(plan));
            var duty = new DriverDuty(plan.NextDriverDutyId(), null);
            plan.AddDriverDuty(duty);
            return duty;
        }

        /// <summary>
        /// Renumbers all driver duties in running order: sorted by effective start time
        /// (the <c>StartTime</c> extension), then end time, then the duty's first session/day number
        /// so split-session variants of one duty stay in
        /// order — a 1,3,5 duty before its 2,4,6 twin. Each duty's <see cref="DriverDuty.Identity"/> is set
        /// to its ordinal — "1", "2", … Empty duties (no start time) sort last.
        /// </summary>
        /// <remarks>
        /// Duties with <see cref="DriverDuty.IsExcludedFromRenumbering"/> keep their identity, and the
        /// ordinal walk steps over the numbers they hold. Excluding the duty alone would not be enough:
        /// the walk would hand a pinned duty's number to somebody else and produce two booklets with the
        /// same number, which is worse than not renumbering at all — the pile is sorted by exactly that
        /// number. Reserved identities are compared as strings, so a non-numeric pin such as "L1"
        /// reserves nothing and simply never collides.
        /// </remarks>
        public void RenumberDriverDuties()
        {
            plan = plan.ValueOrException(nameof(plan));

            var reserved = plan.DriverDuties
                .Where(d => d.IsExcludedFromRenumbering)
                .Select(d => d.Identity)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var ordered = plan.DriverDuties
                .Where(d => !d.IsExcludedFromRenumbering)
                .OrderBy(d => d.StartTime is null)
                .ThenBy(d => d.StartTime)
                .ThenBy(d => d.EndTime)
                .ThenBy(d => d.Sessions.FirstNumber)
                .ToList();

            var number = 1;
            foreach (var duty in ordered)
            {
                while (reserved.Contains(number.ToString(CultureInfo.InvariantCulture))) number++;
                duty.Identity = number++.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// The plan's driver duties in the order their booklets are printed and the pile is kept:
        /// numeric identities first in numeric order, then any non-numeric identity alphabetically.
        /// </summary>
        /// <remarks>
        /// This is deliberately not the running order used by <c>RenumberDriverDuties</c>. Renumbering
        /// used to make the two identical, so printing in running order produced a number-ordered stack
        /// by coincidence; a pinned number (<see cref="DriverDuty.IsExcludedFromRenumbering"/>) breaks
        /// that, and the pile order is the one that must win — the stack should need no hand-sorting.
        /// Sorting on the parsed number is what keeps "10" after "2" rather than before it.
        /// </remarks>
        public IReadOnlyList<DriverDuty> DriverDutiesInPrintOrder =>
        [
            .. plan.DriverDuties
                .OrderBy(d => int.TryParse(d.Identity, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) ? 0 : 1)
                .ThenBy(d => int.TryParse(d.Identity, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0)
                .ThenBy(d => d.Identity, StringComparer.CurrentCulture)
        ];

        /// <summary>
        /// Gets the train parts that can be added to the given duty: the traction parts (worked by a
        /// locomotive or trainset) that do not overlap in time with the duty's parts, are not already in it,
        /// and are not worked by another duty on a session this duty also runs. For a non-empty duty only
        /// parts departing at or after the duty's last arrival are offered (the natural continuation — from
        /// any station, since the driver walks between parts). Ordered by departure, then train number.
        /// </summary>
        /// <param name="duty">The duty being built.</param>
        /// <returns>The candidate parts.</returns>
        public IReadOnlyList<ScheduledTrainPart> CandidatePartsFor(DriverDuty duty)
        {
            plan = plan.ValueOrException(nameof(plan));
            duty = duty.ValueOrException(nameof(duty));

            IEnumerable<ScheduledTrainPart> candidates = plan.TractionParts
                .Where(p => !duty.Parts.Contains(p))
                .Where(p => !p.IsOverlapping(duty.Parts))
                .Where(p => !plan.DriverDuties.Any(other =>
                    !other.Equals(duty) && other.Parts.Contains(p) && other.Sessions.Overlaps(duty.Sessions)));

            if (duty.LastArrival is { } lastArrival)
                candidates = candidates.Where(p => p.From.Departure >= lastArrival);

            return
            [
                .. candidates
                    .OrderBy(p => p.From.Departure)
                    .ThenBy(p => p.Train.Number)
            ];
        }

        /// <summary>
        /// The scheduled train parts that are worked by a traction unit (locomotive or trainset), i.e. the
        /// segments a driver drives. A part is included when the schedule it belongs to has an assigned
        /// traction vehicle.
        /// </summary>
        private IEnumerable<ScheduledTrainPart> TractionParts =>
            plan.Schedules
                .SelectMany(s => s.Parts)
                .Where(p => plan.ScheduledObjectsFor(p).Any(so => so.IsTraction));

        private int NextDriverDutyId() =>
            (plan.DriverDuties.Count == 0 ? 0 : plan.DriverDuties.Max(d => d.Id)) + 1;
    }
}
