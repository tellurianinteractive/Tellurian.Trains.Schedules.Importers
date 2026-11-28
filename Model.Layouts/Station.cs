using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A station: trains stop here when the call says so, to meet, be overtaken, or exchange passengers or cargo. It is
/// manned, or worked by the loco drivers themselves when <see cref="IsManned"/> is false.
/// </summary>
public sealed class Station : OperationLocation
{
    /// <summary>Creates a station.</summary>
    public Station(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>Creates one with no name or signature yet, to be given with an object initializer.</summary>
    [JsonConstructor]
    public Station() : base() { }

    /// <summary>A station that stands for the world outside the layout: a shadow station or fiddle yard.</summary>
    public bool IsShadow { get; set; }

    /// <summary>The station has its own staff in this layout; otherwise the loco drivers work it themselves.</summary>
    public bool IsManned { get; set; }

    /// <summary>The station has a turntable, so a locomotive can be turned here.</summary>
    public bool HasTurntable { get; set; }

    /// <summary>
    /// The regions and countries represented by this station, mostly by a shadow station. Each is an entry of the
    /// layout's catalogue, <see cref="Layout.Regions"/>; a file keeps only <see cref="RegionIds"/> here.
    /// </summary>
    [JsonIgnore]
    public IList<Region> Regions { get; set; } = [];

    /// <summary>
    /// The ids of the regions in <see cref="Regions"/>, the form they are written in. Setting this only records the
    /// ids; the regions are looked up in the layout's catalogue once the whole layout has been read.
    /// </summary>
    [JsonIgnore]
    public IList<int> RegionIds
    {
        get => [.. Regions.Select(region => region.Id)];
        set => UnresolvedRegionIds = value;
    }

    /// <summary>
    /// <see cref="RegionIds"/> as written: null, and so not written, for a station that stands for no region, which is
    /// every station but the few shadow yards.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("regionIds")]
    public IList<int>? WrittenRegionIds
    {
        get => Regions.Count > 0 ? RegionIds : UnresolvedRegionIds.Count > 0 ? UnresolvedRegionIds : null;
        set => UnresolvedRegionIds = value ?? [];
    }

    /// <summary>The region ids read that have yet to be looked up in the layout's catalogue; emptied once resolved.</summary>
    internal IList<int> UnresolvedRegionIds { get; set; } = [];

    /// <summary>Always true at a shadow station, which stands for everything beyond the layout; otherwise as set.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override bool HasPassengerExchange
    {
        get => IsShadow || base.HasPassengerExchange;
        set => base.HasPassengerExchange = value;
    }

    /// <summary>Always true at a shadow station; otherwise as set.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override bool HasCargoExchange
    {
        get => IsShadow || base.HasCargoExchange;
        set => base.HasCargoExchange = value;
    }
}
