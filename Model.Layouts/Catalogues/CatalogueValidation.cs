using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>
/// Checks user catalogue entries before they are saved (CA-2), against the catalogue they will be part of.
/// An entry may replace the catalogue entry with the same id, so it isn't compared with that one.
/// </summary>
public static class CatalogueValidation
{
    /// <summary>Validates a standard: short name, scale, gauges, lengths, track colour, default end profiles and countries.</summary>
    public static IReadOnlyList<CatalogueProblem> Validate(ModuleStandard standard, Catalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(standard);
        ArgumentNullException.ThrowIfNull(catalogue);
        var problems = new List<CatalogueProblem>();
        if (string.IsNullOrWhiteSpace(standard.ShortName)) problems.Add(new(CatalogueProblemCode.ShortNameMissing, nameof(standard.ShortName)));
        else if (catalogue.Standards.Any(s => s.Id != standard.Id && Same(s.ShortName, standard.ShortName))) problems.Add(new(CatalogueProblemCode.ShortNameInUse, nameof(standard.ShortName)));
        if (standard.Scale is null || catalogue.FindScale(standard.Scale.ShortName) is null) problems.Add(new(CatalogueProblemCode.ScaleUnknown, nameof(standard.Scale)));
        // Gauge problems are reported on the normal gauge, the first of the list and the field an editor always shows.
        if (standard.Gauges is not { Count: > 0 } gauges) problems.Add(new(CatalogueProblemCode.GaugesMissing, nameof(standard.NormalGauge)));
        else if (gauges.Any(g => g.Width <= 0)) problems.Add(new(CatalogueProblemCode.NotPositive, nameof(standard.NormalGauge)));
        else if (gauges.Select(g => Math.Round(g.Width, 1)).Distinct().Count() != gauges.Count) problems.Add(new(CatalogueProblemCode.GaugeRepeated, nameof(standard.NormalGauge)));
        Positive(standard.RailTopHeight, nameof(standard.RailTopHeight));
        Positive(standard.OperatorSpaceDepth, nameof(standard.OperatorSpaceDepth));
        Positive(standard.DefaultCurveRadius, nameof(standard.DefaultCurveRadius));
        Positive(standard.MinimumReverseCurveRadius, nameof(standard.MinimumReverseCurveRadius));
        if (standard.MinimumClearance < 0) problems.Add(new(CatalogueProblemCode.NotPositive, nameof(standard.MinimumClearance)));
        if (standard.TrackColor is not null && !IsHexColor(standard.TrackColor)) problems.Add(new(CatalogueProblemCode.TrackColorInvalid, nameof(standard.TrackColor)));
        OfStandard(standard.DefaultSingleTrackEndProfileId, nameof(standard.DefaultSingleTrackEndProfileId));
        OfStandard(standard.DefaultDoubleTrackEndProfileId, nameof(standard.DefaultDoubleTrackEndProfileId));
        if (standard.Countries is { } countries && !countries.All(IsCountryCode)) problems.Add(new(CatalogueProblemCode.CountryInvalid, nameof(standard.Countries)));
        return problems;

        void Positive(double? value, string property)
        {
            if (value is <= 0) problems.Add(new(CatalogueProblemCode.NotPositive, property));
        }

        // A default end profile is one of the standard's own or of a standard it extends, as the edited standard states it.
        void OfStandard(Guid? id, string property)
        {
            if (id is not { } profileId) return;
            HashSet<Guid> standardIds = standard.ExtendsStandardId is { } extendedId ? catalogue.StandardAndExtendedIds(extendedId) : [];
            if (standard.Id is { } ownId) standardIds.Add(ownId);
            if (catalogue.FindEndProfile(profileId)?.Standards.Any(s => standardIds.Contains(s.Id)) != true)
                problems.Add(new(CatalogueProblemCode.EndProfileNotOfStandard, property));
        }
    }

    /// <summary>Validates an add-on: a short name unique among the add-ons of its base standard, and a known base standard.</summary>
    public static IReadOnlyList<CatalogueProblem> Validate(StandardAddOn addOn, Catalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(addOn);
        ArgumentNullException.ThrowIfNull(catalogue);
        var problems = new List<CatalogueProblem>();
        if (string.IsNullOrWhiteSpace(addOn.ShortName)) problems.Add(new(CatalogueProblemCode.ShortNameMissing, nameof(addOn.ShortName)));
        else if (catalogue.AddOns.Any(a => a.Id != addOn.Id && a.BaseStandardId == addOn.BaseStandardId && Same(a.ShortName, addOn.ShortName)))
            problems.Add(new(CatalogueProblemCode.ShortNameInUse, nameof(addOn.ShortName)));
        if (catalogue.FindStandard(addOn.BaseStandardId) is null) problems.Add(new(CatalogueProblemCode.BaseStandardUnknown, nameof(addOn.BaseStandardId)));
        return problems;
    }

    /// <summary>Validates an override; <paramref name="replacing"/> is the override it replaces when edited, if any.</summary>
    public static IReadOnlyList<CatalogueProblem> Validate(EndProfileCompatibilityOverride entry, Catalogue catalogue, EndProfileCompatibilityOverride? replacing = null)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(catalogue);
        var problems = new List<CatalogueProblem>();
        var first = catalogue.FindEndProfile(entry.FirstEndProfileId);
        var second = catalogue.FindEndProfile(entry.SecondEndProfileId);
        if (first is null) problems.Add(new(CatalogueProblemCode.EndProfileUnknown, nameof(entry.FirstEndProfileId)));
        if (second is null) problems.Add(new(CatalogueProblemCode.EndProfileUnknown, nameof(entry.SecondEndProfileId)));
        if (entry.FirstEndProfileId == entry.SecondEndProfileId) problems.Add(new(CatalogueProblemCode.SameEndProfile, nameof(entry.SecondEndProfileId)));
        else if (first is not null && second is not null && !Same(first.Scale, second.Scale)) problems.Add(new(CatalogueProblemCode.DifferentScales, nameof(entry.SecondEndProfileId)));
        var isSamePair = replacing?.Applies(entry.FirstEndProfileId, entry.SecondEndProfileId) == true;
        if (!isSamePair && catalogue.Overrides.Any(o => o.Applies(entry.FirstEndProfileId, entry.SecondEndProfileId)))
            problems.Add(new(CatalogueProblemCode.OverrideExists, nameof(entry.SecondEndProfileId)));
        return problems;
    }

    /// <summary>
    /// Checks every entry of the user's catalogue against the catalogue it is part of. Entries are checked when
    /// they are saved, but can become invalid later, for example when an end profile they refer to is deleted
    /// or an imported entry takes a short name.
    /// </summary>
    public static IReadOnlyList<CatalogueEntryProblem> ValidateUserEntries(Catalogue user, Catalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(catalogue);
        return
        [
            .. user.Standards.SelectMany(s => Validate(s, catalogue).Select(p => new CatalogueEntryProblem(CatalogueEntryKind.Standard, s.Id ?? Guid.Empty, null, p))),
            .. user.AddOns.SelectMany(a => Validate(a, catalogue).Select(p => new CatalogueEntryProblem(CatalogueEntryKind.AddOn, a.Id, null, p))),
            .. user.Overrides.SelectMany(o => Validate(o, catalogue, replacing: o).Select(p => new CatalogueEntryProblem(CatalogueEntryKind.Override, o.FirstEndProfileId, o.SecondEndProfileId, p))),
        ];
    }

    /// <summary>An ISO 3166 two-letter country code in capitals, e.g. <c>SE</c>.</summary>
    public static bool IsCountryCode(string? value) => value is { Length: 2 } && value.All(char.IsAsciiLetterUpper);

    /// <summary>A CSS hex colour: <c>#rgb</c> or <c>#rrggbb</c>.</summary>
    public static bool IsHexColor(string? value) =>
        value is { Length: 4 or 7 } && value[0] == '#' && value[1..].All(char.IsAsciiHexDigit)
        && int.TryParse(value[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);

    private static bool Same(string a, string b) => string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
}
