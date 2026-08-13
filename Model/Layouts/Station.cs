using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A manned <see cref="OperationLocation"/> (or driver-operated when <see cref="IsManned"/> is false).
/// </summary>
/// <remarks>
/// A train stops here when the call says so (<see cref="Timetables.StationCall.IsStop"/>) — to meet,
/// be overtaken, or exchange passengers or cargo. This is the only location type with cargo exchange.
/// </remarks>
public class Station : OperationLocation
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="signature"></param>
    public Station(int id, string name, string signature) : base(id, name, signature) { }
    /// <summary>
    /// Gets or sets a value indicating whether this is a shadow station — a terminal station representing external stations or regions beyond the modelled railway.
    /// </summary>
    public bool IsShadow { get; set; }

    /// <summary>
    /// If false, indicate a station where train drivers has to operate it themselves, e.g. a freight terminal och a small shadow station.
    /// </summary>
    public bool IsManned { get; set; }

    /// <summary>
    /// Gets or sets whether the station has a turntable. A locomotive that cannot be driven equally well
    /// in both directions — a steam locomotive above all — can only be turned where there is one, so this
    /// is what decides whether a train part may ask for a turn on arrival
    /// (<see cref="Schedules.TractionOptions.TurnLoco"/>).
    /// </summary>
    public bool HasTurntable { get; set; }

    /// <summary>
    /// Gets or sets the regions and countries represented by this station. Mostly meaningful for
    /// shadow stations (<see cref="IsShadow"/>), which represent external stations or regions and
    /// are used for cargo flow routing. Seldom used for ordinary stations. Each entry is an entry of
    /// the layout's own catalogue, <see cref="Layout.Regions"/>, which is where it is stored; a plan
    /// keeps only <see cref="RegionIds"/> here.
    /// </summary>
    public IList<Region> Regions { get; set; } = [];

    /// <summary>
    /// Gets or sets the ids of the regions in <see cref="Regions"/> — the form they are written to a
    /// plan in, each region itself being written once, in the layout's catalogue.
    /// </summary>
    /// <remarks>
    /// Setting this only records the ids: the regions are looked up, and <see cref="Regions"/> filled
    /// in, by <c>Layout.ResolveCatalogueReferences</c> once the whole layout has been read, because the
    /// catalogue need not have been read by the time a station is.
    /// </remarks>
    [JsonConverter(typeof(Schedules.IdListConverter))]
    public IList<int> RegionIds
    {
        get => [.. Regions.Select(region => region.Id)];
        set => UnresolvedRegionIds = value;
    }

    /// <summary>
    /// The region ids read from a plan that have yet to be looked up in the layout's catalogue.
    /// Not public, so neither JSON nor EF Core sees it; emptied once the regions are resolved.
    /// </summary>
    internal IList<int> UnresolvedRegionIds { get; set; } = [];

    /// <summary>
    /// Always true at a shadow station, otherwise as configured. A shadow station stands for everything
    /// beyond the modelled railway, so whatever a train brings there has somewhere to come from and go
    /// to — passengers included.
    /// </summary>
    public override bool HasPassengerExchange
    {
        get => IsShadow || base.HasPassengerExchange;
        set => base.HasPassengerExchange = value;
    }

    /// <summary>
    /// Always true at a shadow station, otherwise as configured. See
    /// <see cref="HasPassengerExchange"/> for why a shadow station exchanges everything.
    /// </summary>
    public override bool HasCargoExchange
    {
        get => IsShadow || base.HasCargoExchange;
        set => base.HasCargoExchange = value;
    }

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected Station() : base() { }
}

/// <summary>
/// 
/// </summary>
public static class StationExtensions
{
    extension(Station station)
    {
        /// <summary>
        /// Adds a <see cref="Region"/> if not already present.
        /// </summary>
        /// <param name="region"></param>
        public void Add(Region region)
        {
            if (station.Regions.Contains(region)) return;
            station.Regions.Add(region);
        }
    }
}

