namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// Builds the groups of the Vehicle contributors report: every rolling stock item listed in the Vehicle owners
/// tab, arranged for one kind of reader.
/// </summary>
/// <remarks>
/// Nothing is left out whichever the arrangement. An item that cannot be put under a station — because it is not
/// in operation — or under a participant — because nobody brings it — gets a group of its own: those are exactly
/// the items a planner still has to do something about. The items nobody has booked yet are printed first, since
/// finding somebody to bring them is what the arrangement by owner is used for before a meeting.
/// </remarks>
public static class VehicleContributorsList
{
    /// <summary>Builds the report's groups, in the order they are printed.</summary>
    /// <param name="plan">The plan whose rolling stock is listed.</param>
    /// <param name="grouping">How the rolling stock is arranged.</param>
    /// <param name="settings">Whether the layout counts in sessions or days, and how many there are.</param>
    public static IReadOnlyList<VehicleContributorsGroup> Create(
        Plan plan, VehicleContributorsGrouping grouping, SessionsSettings settings)
    {
        plan = plan.ValueOrException(nameof(plan));
        settings = settings.ValueOrException(nameof(settings));

        // In the Vehicle owners tab's order, which every later ordering keeps for its ties.
        var items = plan.RollingStock
            .OrderBy(vehicle => vehicle.ObjectType)
            .ThenBy(vehicle => vehicle.Designation, StringComparer.CurrentCulture)
            .ThenBy(vehicle => vehicle.Id)
            .Select(vehicle => new Item(
                vehicle,
                vehicle.Start(settings.UseDaysInsteadOfSessionNumbers, settings.MaxNumberOfSessions),
                plan.ContributionFor(vehicle)))
            .ToList();

        var groups = grouping switch
        {
            VehicleContributorsGrouping.OperationLocation => ByOperationLocation(plan, items),
            VehicleContributorsGrouping.Contributor => ByContributor(plan, items),
            _ => ByDccAddress(plan, items),
        };
        return [.. groups.Where(group => group.Lines.Count > 0)];
    }

    // A station's page lists what is set up there, a row per unit brought as in the other arrangements: the
    // primary unit, then its spares right below it, so whoever has to find a replacement sees who has one and
    // its address.
    private static IEnumerable<VehicleContributorsGroup> ByOperationLocation(Plan plan, IReadOnlyList<Item> items)
    {
        var groups = items
            .Where(item => item.Start is not null)
            .GroupBy(item => item.Start!.Location)
            .OrderBy(group => group.Key.Name, StringComparer.CurrentCulture)
            .Select(group => new VehicleContributorsGroup(
                VehicleContributorsGroupKind.OperationLocation,
                group.Key.Name,
                [.. group
                    .OrderBy(item => item.Start!.FirstPosition)
                    .ThenBy(item => item.Start!.Departure)
                    .SelectMany(item => UnitLinesOf(plan, item))]));
        return [.. groups, new(VehicleContributorsGroupKind.NotInOperation, null,
            [.. items.Where(item => item.Start is null).SelectMany(item => UnitLinesOf(plan, item))])];
    }

    // A participant's page lists the units they bring, the ones they set up themselves first. The items nobody has
    // booked come before everyone's pages.
    private static IEnumerable<VehicleContributorsGroup> ByContributor(Plan plan, IReadOnlyList<Item> items)
    {
        var notYetBooked = new VehicleContributorsGroup(VehicleContributorsGroupKind.NotYetBooked, null,
            [.. items.Where(item => item.Contributors.Count == 0).Select(item => UnitLine(plan, item, null))]);
        var groups = plan.Participants
            .OrderBy(participant => participant.Name, StringComparer.CurrentCulture)
            .Select(participant => new VehicleContributorsGroup(
                VehicleContributorsGroupKind.Contributor,
                participant.Name,
                [.. items
                    .SelectMany(item => item.Contributors
                        .Where(contributor => contributor.ParticipantId == participant.Id)
                        .Select(contributor => UnitLine(plan, item, contributor)))
                    .OrderBy(line => line.IsPrimary ? 0 : 1)]));
        return [notYetBooked, .. groups];
    }

    // Every unit a DCC address applies to — the traction units; a wagonset is not driven — in address order. The
    // missing addresses, none or one still to be provided, come after the known ones, and last the traction units
    // nobody brings, which have no unit and so no address either.
    private static IEnumerable<VehicleContributorsGroup> ByDccAddress(Plan plan, IReadOnlyList<Item> items)
    {
        var lines = items
            .Where(item => item.Vehicle.NeedsDccAddress)
            .SelectMany(item => UnitLinesOf(plan, item))
            .OrderBy(AddressRank)
            .ThenBy(line => line.Contributor?.DccAddress ?? 0);
        return [new(VehicleContributorsGroupKind.AllUnits, null, [.. lines])];

        static int AddressRank(VehicleContributorLine line) => line switch
        {
            { Contributor: null } => 2,
            { IsDccAddressMissing: true } => 1,
            _ => 0,
        };
    }

    // One line per unit brought, the primary first, or a single line saying nobody brings the item.
    private static IEnumerable<VehicleContributorLine> UnitLinesOf(Plan plan, Item item)
    {
        if (item.Contributors.Count == 0) return [UnitLine(plan, item, null)];
        return item.Contributors.Select(contributor => UnitLine(plan, item, contributor));
    }

    private static VehicleContributorLine UnitLine(Plan plan, Item item, VehicleContributor? contributor) =>
        new(
            item.Vehicle,
            item.Start,
            contributor,
            contributor is null ? null : NameOf(plan, contributor),
            contributor is not null && item.Contribution!.IsPrimary(contributor),
            NotesOf(item, contributor));

    private static string NameOf(Plan plan, VehicleContributor contributor) =>
        plan.ParticipantById(contributor.ParticipantId)?.Name ?? "?";

    // The contributor's note says something about their unit only, so it comes before the one about the item.
    private static IReadOnlyList<string> NotesOf(Item item, VehicleContributor? contributor) =>
        [.. new[] { contributor?.Note, item.Contribution?.Note }
            .Where(note => !string.IsNullOrWhiteSpace(note))
            .Select(note => note!.Trim())
            .Distinct(StringComparer.CurrentCulture)];

    private sealed record Item(ScheduledObject Vehicle, VehicleStart? Start, VehicleContribution? Contribution)
    {
        public IReadOnlyList<VehicleContributor> Contributors { get; } = [.. Contribution?.Contributors ?? []];
    }
}
