using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>
/// Scales, module standards, add-ons, end profiles and compatibility overrides, with lookups.
/// Short names and designations are compared without regard to case.
/// </summary>
public sealed record DrawingCatalogue : ExtensibleRecord
{
    /// <summary>A catalogue without entries.</summary>
    public static DrawingCatalogue Empty { get; } = new();

    /// <summary>The version of the catalogue's content, e.g. of the built-in catalogue (CA-1).</summary>
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

    /// <summary>The end profiles that belong to the standard.</summary>
    public IEnumerable<EndProfile> EndProfilesOf(Guid standardId) => EndProfiles.Where(p => p.Standards.Any(s => s.Id == standardId));

    /// <summary>The catalogue with <paramref name="standard"/> added, or replacing the standard with the same id.</summary>
    public DrawingCatalogue With(ModuleStandard standard) => this with { Standards = [.. Standards.Where(s => standard.Id is null || s.Id != standard.Id), standard] };

    /// <summary>The catalogue with <paramref name="addOn"/> added, or replacing the add-on with the same id.</summary>
    public DrawingCatalogue With(StandardAddOn addOn) => this with { AddOns = [.. AddOns.Where(a => a.Id != addOn.Id), addOn] };

    /// <summary>The catalogue with <paramref name="entry"/> added, replacing <paramref name="replacing"/> and any override of the same pair.</summary>
    public DrawingCatalogue With(EndProfileCompatibilityOverride entry, EndProfileCompatibilityOverride? replacing = null) => this with
    {
        Overrides = [.. Overrides.Where(o => o != replacing && !o.Applies(entry.FirstEndProfileId, entry.SecondEndProfileId)), entry],
    };

    /// <summary>The catalogue with <paramref name="profile"/> added, or replacing the end profile with the same id.</summary>
    public DrawingCatalogue With(EndProfile profile) => this with { EndProfiles = [.. EndProfiles.Where(p => p.Id != profile.Id), profile] };

    /// <summary>The catalogue without the standard with <paramref name="id"/>; its add-ons and end profiles are kept.</summary>
    public DrawingCatalogue WithoutStandard(Guid id) => this with { Standards = [.. Standards.Where(s => s.Id != id)] };

    /// <summary>The catalogue without the add-on with <paramref name="id"/>.</summary>
    public DrawingCatalogue WithoutAddOn(Guid id) => this with { AddOns = [.. AddOns.Where(a => a.Id != id)] };

    /// <summary>The catalogue without <paramref name="entry"/>.</summary>
    public DrawingCatalogue WithoutOverride(EndProfileCompatibilityOverride entry) => this with { Overrides = [.. Overrides.Where(o => o != entry)] };

    /// <summary>
    /// The entries to choose from in <paramref name="country"/> (a two-letter code, or null when not chosen):
    /// without standards that apply only in other countries (CA-6), their add-ons, the end profiles that belong
    /// only to such standards, and overrides of those end profiles. End profiles without a standard are kept.
    /// Drawings that use a removed entry are still read with the whole catalogue.
    /// </summary>
    public DrawingCatalogue ForCountry(string? country) =>
        Keeping(s => s.AppliesTo(country), (p, standardIds) => p.Standards.Count == 0 || p.Standards.Any(s => standardIds.Contains(s.Id)));

    /// <summary>
    /// The entries to choose from in <paramref name="scale"/> (a scale's short name, or null for all scales, AP-9):
    /// the standards and end profiles of that scale, the add-ons of those standards and the overrides of those end
    /// profiles. The scales themselves are all kept. Drawings that use a removed entry are still read with the whole catalogue.
    /// </summary>
    public DrawingCatalogue ForScale(string? scale) =>
        string.IsNullOrWhiteSpace(scale) ? this : Keeping(s => s.Scale is { } standardScale && Same(standardScale.ShortName, scale!), (p, _) => Same(p.Scale, scale!));

    /// <summary>The entries to choose from with the user's settings: <see cref="ForCountry"/> and <see cref="ForScale"/>.</summary>
    public DrawingCatalogue ForChoices(string? country, string? scale) => ForCountry(country).ForScale(scale);

    private DrawingCatalogue Keeping(Func<ModuleStandard, bool> keepStandard, Func<EndProfile, HashSet<Guid>, bool> keepProfile)
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
    /// The catalogue to work with: the built-in entries and the user's own (CA-2), which are kept apart.
    /// A user entry replaces a built-in one with the same id (or, for scales, short name).
    /// </summary>
    public static DrawingCatalogue Merge(DrawingCatalogue builtIn, DrawingCatalogue user)
    {
        ArgumentNullException.ThrowIfNull(builtIn);
        ArgumentNullException.ThrowIfNull(user);
        return new()
        {
            Version = builtIn.Version,
            Scales = Layer(builtIn.Scales, user.Scales, s => s.ShortName.Trim().ToUpperInvariant()),
            // A standard without an id is an entry of its own: it never replaces another, nor is it replaced.
            Standards = Layer(builtIn.Standards, user.Standards, s => s.Id ?? Guid.NewGuid()),
            AddOns = Layer(builtIn.AddOns, user.AddOns, a => a.Id),
            EndProfiles = Layer(builtIn.EndProfiles, user.EndProfiles, p => p.Id),
            Overrides = [.. builtIn.Overrides.Where(b => !user.Overrides.Any(u => u.Applies(b.FirstEndProfileId, b.SecondEndProfileId))), .. user.Overrides],
        };
    }

    private static IReadOnlyList<T> Layer<T, TKey>(IReadOnlyList<T> builtIn, IReadOnlyList<T> user, Func<T, TKey> key)
    {
        var userKeys = user.Select(key).ToHashSet();
        return [.. builtIn.Where(b => !userKeys.Contains(key(b))), .. user];
    }

    private static bool Same(string a, string b) => string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
}
