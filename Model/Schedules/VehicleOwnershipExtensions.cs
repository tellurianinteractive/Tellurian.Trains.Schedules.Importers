namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Who brings the rolling stock of a <see cref="Plan"/> to the meeting: the participant catalogue
/// (<see cref="Plan.Participants"/>) and, per rolling stock item, the participants bringing it
/// (<see cref="Plan.VehicleContributions"/>). Removing a contributor or a participant lives in
/// <c>DeletionRules</c>.
/// </summary>
/// <remarks>
/// None of these persist: a caller that changes the plan through them saves it afterwards (in the app,
/// <c>ScheduleState.SaveAndNotify()</c>).
/// </remarks>
public static class VehicleOwnershipExtensions
{
    extension(ScheduledObject vehicle)
    {
        /// <summary>
        /// Whether the vehicle is rolling stock a participant brings to the meeting: a locomotive, a
        /// trainset, a wagonset or a shunter. A cargo flow is wagons routed by waybills, and cargo is no
        /// vehicle at all.
        /// </summary>
        public bool IsRollingStock =>
            vehicle.ObjectType is ScheduledObjectType.Locomotive or ScheduledObjectType.Trainset or ScheduledObjectType.Wagonset or ScheduledObjectType.Shunter;

        /// <summary>
        /// Whether every unit of the vehicle brought — the spares included — must have a DCC address: true
        /// for a traction unit or a shunter, which is driven on the layout.
        /// </summary>
        public bool NeedsDccAddress => vehicle.IsPowered;

        /// <summary>
        /// Whether <paramref name="address"/> may be stored for a unit of this vehicle: a valid address (see
        /// <see cref="DccAddresses.IsValid"/>) for a traction unit, and none at all for anything else.
        /// </summary>
        /// <param name="address">The address, or <c>null</c> for none.</param>
        public bool AcceptsDccAddress(int? address) =>
            vehicle.NeedsDccAddress ? address is { } value && DccAddresses.IsValid(value) : address is null;

        /// <summary>
        /// Whether each unit of the vehicle brought — the spares included — may be given a number of its own:
        /// true for a traction unit or a shunter. A wagonset's numbers are those of its wagons, kept on the
        /// wagonset itself.
        /// </summary>
        public bool TakesVehicleNumber => vehicle.IsPowered;

        /// <summary>
        /// Where the vehicle is to stand before the meeting begins: the start of the first train part it works
        /// on the first session or day it is in operation, or <c>null</c> when it works no train part at all.
        /// </summary>
        /// <remarks>
        /// When the meeting begins later than 00:00 on its first session/day (see
        /// <see cref="Settings.GeneralSettings.FirstSessionStartTime"/>), the parts that start before then on
        /// that session/day are not worked from the meeting's start, so the vehicle starts at its first part
        /// from then on — later on the first session/day, or else on the next one it works. A vehicle working
        /// nothing from then on starts at its first part all the same.
        /// </remarks>
        /// <param name="useDays">Whether the operating period is counted in days rather than sessions.</param>
        /// <param name="maxSessions">The number of sessions or days in the operating period.</param>
        /// <param name="firstSessionStart">The fast-clock time the first session/day begins; see
        /// the <c>FirstSessionStart</c> extension of <see cref="Settings.GeneralSettings"/>.</param>
        public VehicleStart? Start(bool useDays, int maxSessions, TimeSpan firstSessionStart = default)
        {
            var combinations = vehicle.SessionCombinations(useDays, maxSessions);
            if (combinations.FirstOrDefault() is not { Parts.Count: > 0 } first) return null;
            var worked = combinations.Aggregate(new Sessions(), (all, combination) => all.Or(combination.Sessions));
            var isEverySession = worked.CoversAllWithin(useDays, maxSessions);
            var periodMax = Math.Clamp(maxSessions, 1, useDays ? 7 : 14);
            return Enumerable.Range(1, periodMax)
                .SelectMany(number => combinations
                    .Where(combination => combination.Sessions.Includes(number))
                    .SelectMany(combination => combination.Parts)
                    .Where(part => number > 1 || part.StartTime.Value >= firstSessionStart)
                    .Take(1)
                    .Select(part => new VehicleStart(number, part, isEverySession)))
                .FirstOrDefault()
                ?? new(first.Sessions.FirstNumber, first.Parts[0], isEverySession);
        }
    }

    extension(Plan plan)
    {
        /// <summary>
        /// The plan's rolling stock: the vehicles a participant brings to the meeting (see
        /// <see cref="get_IsRollingStock(ScheduledObject)"/>).
        /// </summary>
        public IEnumerable<ScheduledObject> RollingStock => plan.ScheduledObjects.Where(v => v.IsRollingStock);

        /// <summary>
        /// The contribution recorded for <paramref name="vehicle"/>, or <c>null</c> when nothing has been.
        /// </summary>
        /// <param name="vehicle">The rolling stock item.</param>
        public VehicleContribution? ContributionFor(ScheduledObject vehicle) =>
            plan.VehicleContributions.FirstOrDefault(c => c.ScheduledObjectId == vehicle.Id);

        /// <summary>
        /// The participants bringing <paramref name="vehicle"/>, the primary one first; empty when there are none.
        /// </summary>
        /// <param name="vehicle">The rolling stock item.</param>
        public IReadOnlyList<VehicleContributor> ContributorsOf(ScheduledObject vehicle) =>
            plan.ContributionFor(vehicle) is { } contribution ? [.. contribution.Contributors] : [];

        /// <summary>
        /// The participant with the given <see cref="Participant.Id"/>, or <c>null</c> when there is none.
        /// </summary>
        /// <param name="id">The participant id.</param>
        public Participant? ParticipantById(int id) => plan.Participants.FirstOrDefault(p => p.Id == id);

        /// <summary>
        /// The participant known by <paramref name="name"/>, ignoring case and white space, or <c>null</c>.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        public Participant? ParticipantNamed(string? name) => plan.Participants.FirstOrDefault(p => p.IsNamed(name));

        /// <summary>
        /// The participants whose name starts with <paramref name="typed"/>, or has a later word that does —
        /// the choices a name field offers while it is being typed into. Those whose whole name matches come
        /// first; each group is in name order. Nothing typed offers everyone.
        /// </summary>
        /// <param name="typed">What has been typed so far.</param>
        public IReadOnlyList<Participant> ParticipantsMatching(string? typed) =>
            [.. plan.Participants
                .Select(participant => (Participant: participant, Rank: participant.MatchRank(typed)))
                .Where(match => match.Rank is not null)
                .OrderBy(match => match.Rank)
                .ThenBy(match => match.Participant.Name, StringComparer.CurrentCulture)
                .Select(match => match.Participant)];

        /// <summary>
        /// The participant known by <paramref name="name"/>, added to the catalogue first when nobody is.
        /// Returns <c>null</c> for a blank name, which names nobody.
        /// </summary>
        /// <param name="name">The participant's name.</param>
        public Participant? FindOrAddParticipant(string? name)
        {
            plan = plan.ValueOrException(nameof(plan));
            var normalized = name.NormalizedParticipantName;
            if (normalized.Length == 0) return null;
            if (plan.ParticipantNamed(normalized) is { } existing) return existing;
            var participant = new Participant(plan.NextParticipantId(), normalized);
            plan.Participants.Add(participant);
            return participant;
        }

        /// <summary>
        /// Renames a participant, which renames them wherever they appear. Refused, leaving the name as it was,
        /// for a blank name and for one another participant is already known by.
        /// </summary>
        /// <param name="participant">The participant to rename.</param>
        /// <param name="name">The new name.</param>
        /// <returns><c>true</c> when the participant now has the name; otherwise <c>false</c>.</returns>
        public bool RenameParticipant(Participant participant, string? name)
        {
            plan = plan.ValueOrException(nameof(plan));
            participant = participant.ValueOrException(nameof(participant));
            var normalized = name.NormalizedParticipantName;
            if (normalized.Length == 0) return false;
            if (plan.ParticipantNamed(normalized) is { } holder && !ReferenceEquals(holder, participant)) return false;
            participant.Name = normalized;
            return true;
        }

        /// <summary>
        /// The rolling stock <paramref name="participant"/> brings, each item with whether they are its primary
        /// contributor, in the plan's vehicle order. A contribution whose vehicle is no longer in the plan is
        /// left out.
        /// </summary>
        /// <param name="participant">The participant to ask about.</param>
        public IReadOnlyList<(ScheduledObject Vehicle, bool IsPrimary)> RollingStockBroughtBy(Participant participant) =>
            [.. plan.ScheduledObjects
                .Select(vehicle => (Vehicle: vehicle, Contribution: plan.ContributionFor(vehicle)))
                .Where(item => item.Contribution is not null)
                .SelectMany(item => item.Contribution!.Contributors
                    .Where(contributor => contributor.ParticipantId == participant.Id)
                    .Select(contributor => (item.Vehicle, item.Contribution!.IsPrimary(contributor))))];

        /// <summary>
        /// Adds <paramref name="participant"/> as the last contributor of <paramref name="vehicle"/>: its primary
        /// contributor when it has none yet, otherwise one bringing a spare. Refused, returning <c>null</c>,
        /// for a vehicle that is not rolling stock and for a DCC address the vehicle does not accept (see
        /// <c>AcceptsDccAddress</c>) — a traction unit, a spare included, is not added without one.
        /// </summary>
        /// <param name="vehicle">The rolling stock item.</param>
        /// <param name="participant">The participant bringing it; added to the catalogue when not already there.</param>
        /// <param name="dccAddress">The DCC address of the unit this participant brings, or <c>null</c> for none.</param>
        /// <param name="note">An optional note about what this participant brings.</param>
        /// <param name="vehicleNumber">
        /// The number of the unit this participant brings; ignored for a vehicle that takes none (see
        /// <c>TakesVehicleNumber</c>).
        /// </param>
        public VehicleContributor? AddContributor(ScheduledObject vehicle, Participant participant, int? dccAddress, string? note = null, string? vehicleNumber = null)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            participant = participant.ValueOrException(nameof(participant));
            if (!vehicle.IsRollingStock || !vehicle.AcceptsDccAddress(dccAddress)) return null;
            if (!plan.Participants.Contains(participant)) plan.Participants.Add(participant);
            var contributor = new VehicleContributor(participant.Id, dccAddress, note)
            {
                VehicleNumber = vehicle.TakesVehicleNumber ? vehicleNumber : null,
            };
            plan.ContributionOf(vehicle).Contributors.Add(contributor);
            return contributor;
        }

        /// <summary>
        /// Makes <paramref name="contributor"/> the primary contributor of <paramref name="vehicle"/>, the one who
        /// brings the unit and sets it up; the one it replaces becomes the first to bring a spare.
        /// </summary>
        /// <param name="vehicle">The rolling stock item.</param>
        /// <param name="contributor">One of the vehicle's contributors.</param>
        /// <returns><c>true</c> when the contributor is now the primary one; otherwise <c>false</c>.</returns>
        public bool MakePrimary(ScheduledObject vehicle, VehicleContributor contributor)
        {
            if (plan.ContributionFor(vehicle) is not { } contribution) return false;
            var index = contribution.Contributors.IndexOf(contributor);
            if (index < 0) return false;
            contribution.Contributors.RemoveAt(index);
            contribution.Contributors.Insert(0, contributor);
            return true;
        }

        /// <summary>
        /// Gives <paramref name="contributor"/> another DCC address. Refused for an address the vehicle does not
        /// accept (see <c>AcceptsDccAddress</c>), so a traction unit's address can be changed but never
        /// cleared.
        /// </summary>
        /// <param name="vehicle">The rolling stock item the contributor brings.</param>
        /// <param name="contributor">The contributor.</param>
        /// <param name="address">The new address, or <c>null</c> for none.</param>
        /// <returns><c>true</c> when the address was set; otherwise <c>false</c>.</returns>
        public bool SetDccAddress(ScheduledObject vehicle, VehicleContributor contributor, int? address)
        {
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            contributor = contributor.ValueOrException(nameof(contributor));
            if (!vehicle.AcceptsDccAddress(address)) return false;
            contributor.DccAddress = address;
            return true;
        }

        /// <summary>
        /// Sets the note about <paramref name="vehicle"/> as a whole. A vehicle left with no note and no
        /// contributor keeps no contribution at all, so the plan does not fill up with empty ones.
        /// </summary>
        /// <param name="vehicle">The rolling stock item.</param>
        /// <param name="note">The note; blank clears it.</param>
        public void SetContributionNote(ScheduledObject vehicle, string? note)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            var text = note?.Trim() ?? string.Empty;
            if (text.Length == 0 && plan.ContributionFor(vehicle) is null) return;
            var contribution = plan.ContributionOf(vehicle);
            contribution.Note = text;
            plan.RemoveIfEmpty(contribution);
        }

        /// <summary>
        /// Removes <paramref name="contribution"/> from the plan when it says nothing (see
        /// <see cref="VehicleContributionExtensions.get_IsEmpty(VehicleContribution)"/>).
        /// </summary>
        internal void RemoveIfEmpty(VehicleContribution contribution)
        {
            if (contribution.IsEmpty) plan.VehicleContributions.Remove(contribution);
        }

        // The vehicle's contribution, created when it has none yet.
        private VehicleContribution ContributionOf(ScheduledObject vehicle)
        {
            if (plan.ContributionFor(vehicle) is { } existing) return existing;
            var contribution = new VehicleContribution(vehicle.Id);
            plan.VehicleContributions.Add(contribution);
            return contribution;
        }

        private int NextParticipantId() =>
            plan.Participants.Select(p => p.Id).DefaultIfEmpty(0).Max() + 1;
    }
}
