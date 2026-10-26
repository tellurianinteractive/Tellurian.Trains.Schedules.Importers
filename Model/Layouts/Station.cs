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
    /// The people staffing the station, each with the roles they fill. Only in force while the station
    /// is <see cref="IsManned"/>: the posts are kept either way, so unmanning a station and manning it
    /// again does not lose them (see <c>StationExtensions.ParticipantsNeeded</c>).
    /// </summary>
    public IList<StaffPost> StaffPosts { get; set; } = [];

    /// <summary>
    /// How many shifts a long session is divided into at this station (1–3). The same people are
    /// needed for every shift, so each shift needs the whole of <see cref="StaffPosts"/> again.
    /// </summary>
    public int ShiftCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether the station has a turntable. A locomotive that cannot be driven equally well
    /// in both directions — a steam locomotive above all — can only be turned where there is one, so this
    /// is what decides whether a train part may ask for a turn on arrival
    /// (<see cref="Schedules.TractionOptions.TurnLoco"/>).
    /// </summary>
    public bool HasTurntable { get; set; }

    /// <summary>
    /// Gets or sets which trains starting at this station get a start label. The train's category can
    /// narrow this further (see <see cref="Timetables.TrainCategory.StartLabels"/>); the narrower of the
    /// two decides. None by default.
    /// </summary>
    public Timetables.StartLabelPrinting StartLabels { get; set; }

    /// <summary>
    /// Gets or sets whether the station's dispatch list is divided into one list per direction — the
    /// lines that lead away from the station (see <c>Layout.DirectionsAt</c>) — instead of one list for
    /// the whole station. Only meaningful at a station with a dispatcher, and has no effect where the
    /// station has fewer than two directions.
    /// </summary>
    public bool SplitDispatchByDirection { get; set; }

    /// <summary>
    /// Gets or sets whether passengers change trains here. Only in force while the layout is worked with
    /// passenger tickets (<see cref="Settings.GeneralSettings.UsePassengerTickets"/>): the mark is kept
    /// either way, and <c>OperationLocation.ExchangesTransferringPassengers</c> is what says whether it
    /// counts, so nothing has to combine the two itself.
    /// </summary>
    public bool IsPassengerInterchange { get; set; }

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
        /// The number of participants needed to operate the station: one for each staff post in each
        /// shift, and none while the station is not manned.
        /// </summary>
        public int ParticipantsNeeded =>
            station.IsManned ? station.StaffPosts.Count * Math.Clamp(station.ShiftCount, 1, 3) : 0;

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


/// <summary>
/// Provides extension members for the staffing of a <see cref="Layout"/>.
/// </summary>
public static class LayoutStaffingExtensions
{
    extension(Layout layout)
    {
        /// <summary>The number of participants needed to operate every station of the layout.</summary>
        public int ParticipantsNeeded => layout.OperationLocations.OfType<Station>().Sum(station => station.ParticipantsNeeded);
    }
}
