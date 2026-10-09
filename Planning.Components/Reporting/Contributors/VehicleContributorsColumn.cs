namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>A column of the Vehicle contributors report.</summary>
public enum VehicleContributorsColumn
{
    /// <summary>Whether the unit is the one set up on the layout or a spare.</summary>
    Role,
    /// <summary>The DCC address of the unit.</summary>
    DccAddress,
    /// <summary>The first session or day the item is in operation.</summary>
    FirstSession,
    /// <summary>The location the item is to be set up at.</summary>
    Station,
    /// <summary>The track the item is to be set up on.</summary>
    Track,
    /// <summary>The departure of the first train the item works.</summary>
    Departure,
    /// <summary>Locomotive, trainset or wagonset, followed by how a locomotive or trainset is powered where that is stated.</summary>
    Type,
    /// <summary>The number of the turnus the item runs.</summary>
    Turnus,
    /// <summary>The number of the traction unit the row's owner brings, when they have given one.</summary>
    VehicleNumber,
    /// <summary>The number of units making up the item, when more than one.</summary>
    Count,
    /// <summary>The participant bringing the unit.</summary>
    Owner,
    /// <summary>The notes about the unit and the item.</summary>
    Note,
}

/// <summary>Which columns each kind of group of the Vehicle contributors report prints.</summary>
public static class VehicleContributorsColumns
{
    /// <summary>
    /// The columns where a vehicle is to be set up: printed side by side, so that a vehicle not in operation
    /// can say so once across all of them.
    /// </summary>
    public static IReadOnlyList<VehicleContributorsColumn> Start { get; } =
        [VehicleContributorsColumn.FirstSession, VehicleContributorsColumn.Station, VehicleContributorsColumn.Track, VehicleContributorsColumn.Departure];

    extension(VehicleContributorsColumn column)
    {
        /// <summary>Whether the column is one of the <see cref="Start"/> columns.</summary>
        public bool IsStart => Start.Contains(column);
    }

    extension(VehicleContributorsGroupKind kind)
    {
        /// <summary>
        /// The columns a group of this kind prints, left to right. Each leads with what its reader looks a row
        /// up by, and leaves out what the heading already says: a station's page has no station column, and
        /// a participant's page no owner.
        /// </summary>
        public IReadOnlyList<VehicleContributorsColumn> Columns => kind switch
        {
            // Session first, then departure: the order the station owner sets the vehicles out in.
            VehicleContributorsGroupKind.OperationLocation =>
                [VehicleContributorsColumn.FirstSession, VehicleContributorsColumn.Track, VehicleContributorsColumn.Departure,
                 .. Item, VehicleContributorsColumn.Owner, VehicleContributorsColumn.Role, VehicleContributorsColumn.DccAddress, VehicleContributorsColumn.Note],
            VehicleContributorsGroupKind.NotInOperation =>
                [.. Item, VehicleContributorsColumn.Owner, VehicleContributorsColumn.Role, VehicleContributorsColumn.DccAddress, VehicleContributorsColumn.Note],
            VehicleContributorsGroupKind.Contributor =>
                [VehicleContributorsColumn.Role, .. Item, VehicleContributorsColumn.DccAddress, .. Start, VehicleContributorsColumn.Note],
            VehicleContributorsGroupKind.NotYetBooked =>
                [.. Item, .. Start, VehicleContributorsColumn.Note],
            _ =>
                [VehicleContributorsColumn.DccAddress, .. Item, VehicleContributorsColumn.Owner, VehicleContributorsColumn.Role, .. Start, VehicleContributorsColumn.Note],
        };
    }

    // What the item is: what kind and how it is powered, before the turnus that identifies it with its class,
    // and the number of the very unit brought.
    private static IReadOnlyList<VehicleContributorsColumn> Item { get; } =
        [VehicleContributorsColumn.Type, VehicleContributorsColumn.Turnus, VehicleContributorsColumn.VehicleNumber, VehicleContributorsColumn.Count];
}
