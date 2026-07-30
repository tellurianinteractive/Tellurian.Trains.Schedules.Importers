namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A station that covers destinations for local freight, with what it covers: the on-layout locations
/// worked from it and the off-layout regions it stands for.
/// </summary>
/// <remarks>
/// Both halves are needed to answer the question a driver actually has when holding a wagon card —
/// <em>where does this wagon go from here, and is that on the layout or off it?</em> Either column alone
/// answers only half of it.
/// </remarks>
/// <param name="Station">The station whose shunting yard this is.</param>
/// <param name="ServedLocations">The on-layout locations whose local freight is worked from it.</param>
/// <param name="ServedRegions">The off-layout destinations it stands for.</param>
public sealed record ShuntingYard(
    Station Station,
    IReadOnlyList<OperationLocation> ServedLocations,
    IReadOnlyList<Region> ServedRegions);

/// <summary>
/// Derives a layout's shunting yards from the <see cref="OperationLocation.CargoServedFrom"/> relation
/// and the regions its stations carry.
/// </summary>
public static class ShuntingYardExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// The layout's shunting yards: every station that covers something, with the locations served
        /// from it and the regions it stands for, in layout order.
        /// </summary>
        /// <remarks>
        /// A station qualifies on <em>either</em> relation. Requiring a served location alone would omit
        /// the shadow shunting yard that serves no on-layout location but stands for several regions —
        /// the very station most in need of listing. A station that neither serves a location nor carries
        /// a region is not a shunting yard: an entry with nothing under it says nothing.
        /// </remarks>
        public IReadOnlyList<ShuntingYard> ShuntingYards
        {
            get
            {
                layout = layout.ValueOrException(nameof(layout));

                var servedByShuntingYard = layout.OperationLocations
                    .Where(location => location.CargoServedFrom is not null)
                    .GroupBy(location => location.CargoServedFrom!)
                    .ToDictionary(group => group.Key, group => (IReadOnlyList<OperationLocation>)[.. group]);

                return
                [
                    .. layout.OperationLocations
                        .OfType<Station>()
                        .Select(station => new ShuntingYard(
                            station,
                            servedByShuntingYard.TryGetValue(station, out var served) ? served : [],
                            [.. station.Regions]))
                        .Where(shuntingYard => shuntingYard.ServedLocations.Count > 0 || shuntingYard.ServedRegions.Count > 0)
                ];
            }
        }

        /// <summary>
        /// The stations that may be offered as the serving shunting yard for <paramref name="location"/>:
        /// every station on the layout except the location itself. Shadow stations are included, since a
        /// shadow shunting yard is a perfectly ordinary origin for local freight.
        /// </summary>
        /// <param name="location">The location whose serving shunting yard is being chosen.</param>
        public IEnumerable<Station> CargoServingStationsFor(OperationLocation location) =>
            layout.OperationLocations.OfType<Station>().Where(station => !station.Equals(location));
    }

    extension(OperationLocation location)
    {
        /// <summary>
        /// Whether this location may be served with cargo from a shunting yard: it must exchange cargo,
        /// and it must not be a shadow station — off-layout staging is where traffic originates, not
        /// somewhere local freight is delivered.
        /// </summary>
        public bool CanBeCargoServed =>
            location.HasCargoExchange && location is not Station { IsShadow: true };
    }
}
