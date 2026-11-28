using System.Text.Json;
using Tellurian.Trains.Schedules.Model.Drawings.Measures;
using Tellurian.Trains.Schedules.Model.Drawings.Modules;
using Tellurian.Trains.Schedules.Model.Drawings.Tracks;

namespace Tellurian.Trains.Schedules.Model.Drawings.Files;

/// <summary>
/// The content of a module file (spec §6): the module with its drawing, the end profiles its exits and
/// internal joints use, and the derived measures per drawing, which the Module Registry can take over (spec §10).
/// </summary>
public sealed record ModuleFileContent : ExtensibleRecord
{
    /// <summary>The module, with its drawing.</summary>
    public required Module Module { get; init; }

    /// <summary>Every end profile the module refers to, so a receiver can show and check the ends.</summary>
    public IReadOnlyList<EndProfile> EndProfiles { get; init; } = [];

    /// <summary>Written for receivers; ignored on load, where measures are derived again.</summary>
    public ModuleMeasures? Measures { get; init; }

    /// <summary>
    /// The file content for <paramref name="module"/>: embeds its end profiles and derives the measures.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An end profile the module refers to isn't available.</exception>
    public static ModuleFileContent Create(Module module, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(endProfiles);
        var profiles = module.Drawing.Exits.Select(e => e.EndProfile)
            .Concat(module.Drawing.InternalJoints.Select(j => j.EndProfile).OfType<CatalogueReference>())
            .DistinctBy(r => r.Id)
            .Select(r => endProfiles(r.Id) ?? throw new KeyNotFoundException($"End profile {r.Name} ({r.Id}) isn't available."))
            .ToList();
        return new()
        {
            Module = module,
            EndProfiles = profiles,
            Measures = ModuleMeasurement.Derive(module.Drawing, endProfiles),
        };
    }

    /// <summary>
    /// The content with the length and the stub mark of every track as the drawing gives them,
    /// except those the user has entered (<see cref="StationTrackMeasurement"/>). The fouling distance is that of
    /// <paramref name="standard"/>; <paramref name="catalogue"/> finds the standard's end profiles that the module
    /// doesn't embed. Without a standard a track's length is the whole of it. Unchanged when nothing differs.
    /// </summary>
    public ModuleFileContent WithMeasuredTracks(ModuleStandard? standard, Func<Guid, EndProfile?>? catalogue = null)
    {
        if (Module.Tracks.Count == 0) return this;
        EndProfile? Lookup(Guid id) => FindEndProfile(id) ?? catalogue?.Invoke(id);
        var foulingDistance = standard is null ? null : StationTrackMeasurement.FoulingDistance(standard, Lookup);
        var measured = Module.WithMeasuredTracks(foulingDistance, Lookup);
        return ReferenceEquals(measured, Module) ? this : this with { Module = measured };
    }

    /// <summary>Looks up the embedded end profiles.</summary>
    public EndProfile? FindEndProfile(Guid id) => EndProfiles.FirstOrDefault(p => p.Id == id);

    /// <summary>
    /// The content with each embedded end profile replaced by the one with the same id in <paramref name="current"/>
    /// where they differ, e.g. after the end profile was changed in the catalogue, and the measures derived again.
    /// Embedded end profiles that <paramref name="current"/> doesn't have are kept.
    /// </summary>
    /// <returns>The content, unchanged when nothing differs, and the end profiles that replaced embedded ones.</returns>
    public (ModuleFileContent Content, IReadOnlyList<EndProfile> Updated) WithCurrentEndProfiles(Func<Guid, EndProfile?> current)
    {
        ArgumentNullException.ThrowIfNull(current);
        var updated = EndProfiles
            .Select(embedded => current(embedded.Id) is { } now && !Same(now, embedded) ? now : null)
            .OfType<EndProfile>()
            .ToList();
        if (updated.Count == 0) return (this, []);
        EndProfile? Lookup(Guid id) => updated.FirstOrDefault(p => p.Id == id) ?? FindEndProfile(id) ?? current(id);
        try
        {
            return (Create(Module, Lookup), updated);
        }
        catch (KeyNotFoundException)
        {
            // An end profile the module refers to was never embedded: replace the embedded ones only.
            return (this with { EndProfiles = [.. EndProfiles.Select(p => updated.FirstOrDefault(u => u.Id == p.Id) ?? p)] }, updated);
        }
    }

    /// <summary>Whether two end profiles have the same content, as written to a file.</summary>
    private static bool Same(EndProfile a, EndProfile b) =>
        ReferenceEquals(a, b) || JsonSerializer.Serialize(a, DrawingJsonContext.Default.EndProfile) == JsonSerializer.Serialize(b, DrawingJsonContext.Default.EndProfile);
}
