namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>One row of the summary page: a participant or an operation location, with what it comes to.</summary>
/// <param name="Name">The name of the participant or the operation location.</param>
/// <param name="Owner">The owner of the operation location; <c>null</c> in the summary by owner.</param>
/// <param name="Items">How many items the participant brings, or are set up at the location.</param>
/// <param name="DccAddressesMissing">Whether the participant brings a traction unit without a usable DCC address;
/// only ever set in the summary by owner.</param>
/// <param name="NumbersMissing">Whether the participant brings a traction unit without a number; only ever set in
/// the summary by owner.</param>
public sealed record VehicleContributorsSummaryRow(
    string Name, string? Owner, int Items, bool DccAddressesMissing = false, bool NumbersMissing = false);

/// <summary>
/// The first page of the Vehicle contributors report: who brings how much, or where how much is set up, so that
/// everybody finds their own page in the pile.
/// </summary>
/// <remarks>
/// Only the arrangements by owner and by operation location have one. The list by DCC address is a single list
/// already.
/// </remarks>
public static class VehicleContributorsSummary
{
    /// <summary>Whether the arrangement is introduced by a summary page.</summary>
    public static bool Has(VehicleContributorsGrouping grouping) =>
        grouping is VehicleContributorsGrouping.Contributor or VehicleContributorsGrouping.OperationLocation;

    /// <summary>
    /// The rows of the summary, in alphabetical order: a participant per row with every unit they bring, spares
    /// included, or a location per row with the items set up there, spares not counted since they are not set up.
    /// Items nobody brings, and items not in operation, belong to no one's page and are left out.
    /// </summary>
    /// <param name="groups">The groups of the report.</param>
    public static IReadOnlyList<VehicleContributorsSummaryRow> Create(IEnumerable<VehicleContributorsGroup> groups)
    {
        groups = groups.ValueOrException(nameof(groups));
        return [.. groups
            .Where(group => group.Kind is VehicleContributorsGroupKind.Contributor or VehicleContributorsGroupKind.OperationLocation)
            .Select(Row)
            .OrderBy(row => row.Name, StringComparer.CurrentCulture)];

        static VehicleContributorsSummaryRow Row(VehicleContributorsGroup group) => group.Kind == VehicleContributorsGroupKind.Contributor
            ? new(group.Name ?? string.Empty, null, group.Lines.Count,
                group.Lines.Any(line => line.IsDccAddressMissing),
                group.Lines.Any(line => line.NeedsDccAddress && line.Contributor is { VehicleNumber: null }))
            : new(group.Name ?? string.Empty,
                group.Lines.Select(line => line.Location?.Owner).FirstOrDefault(owner => !string.IsNullOrWhiteSpace(owner)),
                group.Lines.Count(line => !line.IsSpare));
    }

    /// <summary>Splits the rows into pages of what fits, at least one row to a page.</summary>
    /// <param name="rows">The rows, in the order they are printed.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static IReadOnlyList<IReadOnlyList<VehicleContributorsSummaryRow>> BuildPages(
        IReadOnlyList<VehicleContributorsSummaryRow> rows, VehicleContributorsPageGeometry geometry)
    {
        rows = rows.ValueOrException(nameof(rows));
        geometry = geometry.ValueOrException(nameof(geometry));

        var perPage = Math.Max(1, (int)((geometry.PrintableHeightMm - geometry.HeaderHeightMm) / geometry.RowHeightMm));
        return [.. rows.Chunk(perPage).Select(page => (IReadOnlyList<VehicleContributorsSummaryRow>)page)];
    }
}
