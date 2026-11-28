using Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>
/// What follows from a <see cref="ModuleDrawing"/>, worked out the same way wherever it is asked: in Module Draw, in the
/// Module Registry and when a hall plan is read (<c>ModelSurfaceAnalysis.md</c> 4.2.1). Once the drawing can tell a
/// feature, the feature is no longer entered.
/// </summary>
public static class ModuleDrawings
{
    extension(ModuleDrawing drawing)
    {
        /// <summary>The drawing has duck-under areas: the operator passes under the module (MD-19).</summary>
        public bool HasDuckUnder => drawing.DuckUnderAreas.Count > 0;

        /// <summary>The line ends on the module: the drawing has one exit.</summary>
        public bool IsEndOfLine => drawing.Kind == ModuleKind.EndOfLine;

        /// <summary>
        /// The gauges of the module's track, in millimetres, each once and smallest first: those of the tracks of its
        /// exits' end profiles, which <paramref name="endProfiles"/> finds by id. An end profile it can't find adds
        /// nothing. The standard names them (<c>ModuleStandard.Gauges</c>).
        /// </summary>
        public IReadOnlyList<double> Gauges(Func<Guid, EndProfile?> endProfiles) =>
            [.. ExitTracks(drawing, endProfiles).SelectMany(track => track.Gauges).Distinct().Order()];

        /// <summary>
        /// The rail systems of the module's track, each once: those of the tracks of its exits' end profiles, which
        /// <paramref name="endProfiles"/> finds by id. Empty when no end profile is found.
        /// </summary>
        public IReadOnlyList<RailSystem> RailSystems(Func<Guid, EndProfile?> endProfiles) =>
            [.. ExitTracks(drawing, endProfiles).Select(track => track.RailSystem).Distinct().Order()];
    }

    private static IEnumerable<EndProfileTrack> ExitTracks(ModuleDrawing drawing, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(endProfiles);
        return drawing.Exits.Select(exit => endProfiles(exit.EndProfile.Id)).OfType<EndProfile>().SelectMany(profile => profile.Tracks);
    }
}
