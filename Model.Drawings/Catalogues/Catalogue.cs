using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Drawings.Catalogues;

/// <summary>
/// Scales, module standards, add-ons, end profiles and compatibility overrides, with lookups. A catalogue has no id
/// (decision 12 of <c>CrossAppReview.md</c>): it is a set of entries carried together, in a file or from the Module
/// Registry, and each entry has its own GUID. Short names and designations are compared without regard to case.
/// </summary>
public sealed record Catalogue : ExtensibleRecord
{
    /// <summary>A catalogue without entries.</summary>
    public static Catalogue Empty { get; } = new();

    /// <summary>The version of the catalogue's content, as its publisher gives it; null when not stated.</summary>
    public string? Version { get; init; }

    /// <summary>The scales.</summary>
    public IReadOnlyList<Scale> Scales { get; init; } = [];
    /// <summary>The module standards.</summary>
    public IReadOnlyList<ModuleStandard> Standards { get; init; } = [];
    /// <summary>The add-ons to module standards.</summary>
    public IReadOnlyList<StandardAddOn> AddOns { get; init; } = [];
    /// <summary>The end profiles.</summary>
    public IReadOnlyList<EndProfile> EndProfiles { get; init; } = [];
    /// <summary>The compatibility overrides for pairs of end profiles.</summary>
    public IReadOnlyList<EndProfileCompatibilityOverride> Overrides { get; init; } = [];

    /// <summary>What other systems keep about the catalogue, with one entry per system; read and written back unchanged; the Module Registry's entry is named in <see cref="Layouts.ExternalReferences"/>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ExternalReferences { get; init; }

    /// <summary>The scale with the short name, or null.</summary>
    public Scale? FindScale(string shortName) => Scales.FirstOrDefault(s => Same(s.ShortName, shortName));

    /// <summary>The standard with the id, or null.</summary>
    public ModuleStandard? FindStandard(Guid id) => Standards.FirstOrDefault(s => s.Id == id);

    /// <summary>The standard with the short name, or null.</summary>
    public ModuleStandard? FindStandard(string shortName) => Standards.FirstOrDefault(s => Same(s.ShortName, shortName));

    /// <summary>The add-on with the short name to the base standard, or null.</summary>
    public StandardAddOn? FindAddOn(Guid baseStandardId, string shortName) =>
        AddOns.FirstOrDefault(a => a.BaseStandardId == baseStandardId && Same(a.ShortName, shortName));

    /// <summary>The add-ons to the base standard.</summary>
    public IEnumerable<StandardAddOn> AddOnsOf(Guid baseStandardId) => AddOns.Where(a => a.BaseStandardId == baseStandardId);

    /// <summary>The end profile with the id, or null.</summary>
    public EndProfile? FindEndProfile(Guid id) => EndProfiles.FirstOrDefault(p => p.Id == id);

    /// <summary>The end profile with the designation in <paramref name="scale"/> (a scale's short name), or null.</summary>
    public EndProfile? FindEndProfile(string designation, string scale) =>
        EndProfiles.FirstOrDefault(p => Same(p.Designation, designation) && Same(p.Scale, scale));

    /// <summary>
    /// The end profiles of the standard: those that belong to it, and those of the standards it extends, directly or through
    /// others (<see cref="ModuleStandard.ExtendsStandardId"/>).
    /// </summary>
    public IEnumerable<EndProfile> EndProfilesOf(Guid standardId)
    {
        var standardIds = StandardAndExtendedIds(standardId);
        return EndProfiles.Where(p => p.Standards.Any(s => standardIds.Contains(s.Id)));
    }

    /// <summary>
    /// The standard and the standards it extends, nearest first (<see cref="ModuleStandard.ExtendsStandardId"/>). The chain ends at a
    /// standard that extends none, one the catalogue doesn't have, or one already in the chain (a loop).
    /// </summary>
    public IEnumerable<ModuleStandard> StandardAndExtended(Guid standardId)
    {
        var visited = new HashSet<Guid>();
        var standard = FindStandard(standardId);
        while (standard?.Id is { } id && visited.Add(id))
        {
            yield return standard;
            standard = standard.ExtendsStandardId is { } extendedId ? FindStandard(extendedId) : null;
        }
    }

    /// <summary>The ids of <see cref="StandardAndExtended"/>; at least <paramref name="standardId"/>, also when the catalogue lacks it.</summary>
    internal HashSet<Guid> StandardAndExtendedIds(Guid standardId) => [standardId, .. StandardAndExtended(standardId).Select(s => s.Id!.Value)];

    /// <summary>The catalogue with <paramref name="standard"/> added, or replacing the standard with the same id.</summary>
    public Catalogue With(ModuleStandard standard) => this with { Standards = [.. Standards.Where(s => standard.Id is null || s.Id != standard.Id), standard] };

    /// <summary>The catalogue with <paramref name="addOn"/> added, or replacing the add-on with the same id.</summary>
    public Catalogue With(StandardAddOn addOn) => this with { AddOns = [.. AddOns.Where(a => a.Id != addOn.Id), addOn] };

    /// <summary>The catalogue with <paramref name="entry"/> added, replacing <paramref name="replacing"/> and any override of the same pair.</summary>
    public Catalogue With(EndProfileCompatibilityOverride entry, EndProfileCompatibilityOverride? replacing = null) => this with
    {
        Overrides = [.. Overrides.Where(o => o != replacing && !o.Applies(entry.FirstEndProfileId, entry.SecondEndProfileId)), entry],
    };

    /// <summary>The catalogue with <paramref name="profile"/> added, or replacing the end profile with the same id.</summary>
    public Catalogue With(EndProfile profile) => this with { EndProfiles = [.. EndProfiles.Where(p => p.Id != profile.Id), profile] };

    /// <summary>The catalogue without the standard with <paramref name="id"/>; its add-ons and end profiles are kept.</summary>
    public Catalogue WithoutStandard(Guid id) => this with { Standards = [.. Standards.Where(s => s.Id != id)] };

    /// <summary>The catalogue without the add-on with <paramref name="id"/>.</summary>
    public Catalogue WithoutAddOn(Guid id) => this with { AddOns = [.. AddOns.Where(a => a.Id != id)] };

    /// <summary>The catalogue without <paramref name="entry"/>.</summary>
    public Catalogue WithoutOverride(EndProfileCompatibilityOverride entry) => this with { Overrides = [.. Overrides.Where(o => o != entry)] };

    /// <summary>
    /// The entries to choose from in <paramref name="country"/> (a two-letter code, or null when not chosen):
    /// without standards that apply only in other countries (CA-6), their add-ons, the end profiles that belong
    /// only to such standards, and overrides of those end profiles. End profiles without a standard are kept.
    /// Drawings that use a removed entry are still read with the whole catalogue.
    /// </summary>
    public Catalogue ForCountry(string? country) =>
        Keeping(s => s.AppliesTo(country), (p, standardIds) => p.Standards.Count == 0 || p.Standards.Any(s => standardIds.Contains(s.Id)));

    /// <summary>
    /// The entries to choose from in <paramref name="scale"/> (a scale's short name, or null for all scales, AP-9):
    /// the standards and end profiles of that scale, the add-ons of those standards and the overrides of those end
    /// profiles. The scales themselves are all kept. Drawings that use a removed entry are still read with the whole catalogue.
    /// </summary>
    public Catalogue ForScale(string? scale) =>
        string.IsNullOrWhiteSpace(scale) ? this : Keeping(s => s.Scale is { } standardScale && Same(standardScale.ShortName, scale!), (p, _) => Same(p.Scale, scale!));

    /// <summary>The entries to choose from with the user's settings: <see cref="ForCountry"/> and <see cref="ForScale"/>.</summary>
    public Catalogue ForChoices(string? country, string? scale) => ForCountry(country).ForScale(scale);

    private Catalogue Keeping(Func<ModuleStandard, bool> keepStandard, Func<EndProfile, HashSet<Guid>, bool> keepProfile)
    {
        var standards = Standards.Where(keepStandard).ToList();
        var ids = standards.Where(s => s.Id is not null).Select(s => s.Id!.Value).ToHashSet();
        var profiles = EndProfiles.Where(p => keepProfile(p, ids)).ToList();
        if (standards.Count == Standards.Count && profiles.Count == EndProfiles.Count) return this;
        var profileIds = profiles.Select(p => p.Id).ToHashSet();
        return this with
        {
            Standards = standards,
            AddOns = [.. AddOns.Where(a => ids.Contains(a.BaseStandardId))],
            EndProfiles = profiles,
            Overrides = [.. Overrides.Where(o => profileIds.Contains(o.FirstEndProfileId) && profileIds.Contains(o.SecondEndProfileId))],
        };
    }

    /// <summary>
    /// <paramref name="imported"/> taken into <paramref name="current"/> (decision 12): an entry with a GUID the current
    /// catalogue has replaces that entry, and one with a new GUID is added. Scales are matched by short name and ratio,
    /// overrides by the pair of end profiles they apply to. The version is the imported one's, when it states one.
    /// </summary>
    public static Catalogue Merge(Catalogue current, Catalogue imported)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(imported);
        return new()
        {
            Version = imported.Version ?? current.Version,
            Scales = Layer(current.Scales, imported.Scales, s => (s.ShortName.Trim().ToUpperInvariant(), s.Denominator)),
            // A standard without an id is an entry of its own: it never replaces another, nor is it replaced.
            Standards = Layer(current.Standards, imported.Standards, s => s.Id ?? Guid.NewGuid()),
            AddOns = Layer(current.AddOns, imported.AddOns, a => a.Id),
            EndProfiles = Layer(current.EndProfiles, imported.EndProfiles, p => p.Id),
            Overrides = [.. current.Overrides.Where(c => !imported.Overrides.Any(i => i.Applies(c.FirstEndProfileId, c.SecondEndProfileId))), .. imported.Overrides],
        };
    }

    /// <summary>The entries of <paramref name="current"/> that <paramref name="imported"/> doesn't replace, in their order, followed by the imported ones.</summary>
    private static IReadOnlyList<T> Layer<T, TKey>(IReadOnlyList<T> current, IReadOnlyList<T> imported, Func<T, TKey> key)
    {
        var importedKeys = imported.Select(key).ToHashSet();
        return [.. current.Where(c => !importedKeys.Contains(key(c))), .. imported];
    }

    private static bool Same(string a, string b) => string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
}
