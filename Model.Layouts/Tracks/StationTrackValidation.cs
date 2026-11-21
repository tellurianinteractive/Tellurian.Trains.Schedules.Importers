using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tracks;

/// <summary>What is wrong with a module's list of tracks; UIs show a text per code.</summary>
public enum StationTrackProblemCode
{
    /// <summary>The module has no track in its list. Every module has at least one: a through track or a track ending.</summary>
    NoTracks,

    /// <summary>A track has no designation.</summary>
    DesignationMissing,

    /// <summary>Two tracks of the module have the same designation.</summary>
    DesignationNotUnique,

    /// <summary>A track element refers to a track that isn't in the list.</summary>
    UnknownTrack,

    /// <summary>The module has tracks in its list, but a track element belongs to none of them.</summary>
    ElementWithoutTrack,

    /// <summary>No track element belongs to the track, so its length can't be worked out.</summary>
    TrackWithoutElements,

    /// <summary>Trains are planned to use the track, but no exit can be reached from it.</summary>
    ScheduledTrackReachesNoExit,
}

/// <summary>A problem with the track <paramref name="TrackId"/> or the track element <paramref name="ElementId"/>.</summary>
public sealed record StationTrackProblem(StationTrackProblemCode Code, int? TrackId = null, Guid? ElementId = null);

/// <summary>
/// Checks a module's list of tracks against its drawing (spec MD-8). A turnout, crossing or double slip is shared by
/// the tracks that meet in it and refers to one of them; that isn't a problem.
/// </summary>
public static class StationTrackValidation
{
    /// <summary>Validates the module's tracks against its drawing.</summary>
    /// <param name="module">The module to check.</param>
    /// <param name="endProfiles">Looks up an end profile by id; used to find which tracks reach an exit.</param>
    /// <returns>The problems found; empty when there are none.</returns>
    public static IReadOnlyList<StationTrackProblem> Validate(Module module, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(endProfiles);
        var problems = new List<StationTrackProblem>();
        var drawing = module.Drawing;
        var tracks = module.Tracks;
        var ids = tracks.Select(t => t.Id).ToHashSet();

        // Only an operation location needs tracks; a module of plain line may have none (decided 2026-11-21).
        if (tracks.Count == 0 && module.OperationLocation is not null) problems.Add(new(StationTrackProblemCode.NoTracks));
        problems.AddRange(tracks.Where(t => string.IsNullOrWhiteSpace(t.Designation)).Select(t => new StationTrackProblem(StationTrackProblemCode.DesignationMissing, t.Id)));
        problems.AddRange(tracks
            .Where(t => !string.IsNullOrWhiteSpace(t.Designation))
            .GroupBy(t => t.Designation.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Select(t => new StationTrackProblem(StationTrackProblemCode.DesignationNotUnique, t.Id))));

        foreach (var element in drawing.TrackElements)
        {
            if (element.StationTrackId is { } id)
            {
                if (!ids.Contains(id)) problems.Add(new(StationTrackProblemCode.UnknownTrack, id, element.Id));
            }
            else if (tracks.Count > 0 && element is not BufferStop)
            {
                problems.Add(new(StationTrackProblemCode.ElementWithoutTrack, ElementId: element.Id));
            }
        }

        var network = new TrackNetwork(drawing);
        foreach (var track in tracks)
        {
            var lines = network.LinesOfTrack(track.Id).ToList();
            if (lines.Count == 0)
                problems.Add(new(StationTrackProblemCode.TrackWithoutElements, track.Id));
            else if (track.IsScheduled && !network.ReachableFrom(lines).Any(l => l.Ends.Any(e => TrackNetwork.IsAtExit(e, drawing, endProfiles))))
                problems.Add(new(StationTrackProblemCode.ScheduledTrackReachesNoExit, track.Id));
        }
        return problems;
    }
}
