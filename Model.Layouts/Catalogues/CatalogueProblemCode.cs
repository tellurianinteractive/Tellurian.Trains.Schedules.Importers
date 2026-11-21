namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>Why a catalogue entry can't be saved; UIs show a text per code.</summary>
public enum CatalogueProblemCode
{
    /// <summary>The short name is empty.</summary>
    ShortNameMissing,

    /// <summary>Another entry of the same kind (and base standard, for add-ons) has the short name.</summary>
    ShortNameInUse,

    /// <summary>The scale isn't in the catalogue.</summary>
    ScaleUnknown,

    /// <summary>A length that must be greater than zero isn't.</summary>
    NotPositive,

    /// <summary>A default end profile doesn't exist or doesn't belong to the standard.</summary>
    EndProfileNotOfStandard,

    /// <summary>The track colour isn't a CSS hex colour.</summary>
    TrackColorInvalid,
    /// <summary>An add-on's base standard isn't in the catalogue.</summary>
    BaseStandardUnknown,
    /// <summary>An override's end profile isn't in the catalogue.</summary>
    EndProfileUnknown,

    /// <summary>An override needs two different end profiles.</summary>
    SameEndProfile,

    /// <summary>An override's end profiles are of different scales.</summary>
    DifferentScales,

    /// <summary>There already is an override for the pair.</summary>
    OverrideExists,

    /// <summary>A country isn't an ISO 3166 two-letter code, e.g. <c>SE</c>.</summary>
    CountryInvalid,

    /// <summary>A standard states no gauge.</summary>
    GaugesMissing,

    /// <summary>A standard has two gauges of the same width.</summary>
    GaugeRepeated,
}
