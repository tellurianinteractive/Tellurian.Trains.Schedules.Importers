using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;
using Tellurian.Trains.Schedules.Model.Layouts.Tracks;
using StationTrack = Tellurian.Trains.Schedules.Model.Layouts.StationTrack;
using static Tellurian.Trains.Schedules.Model.Layouts.LayoutPartCopying;

namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>
/// Changes to a drawn module's ends and shape: another end profile at an exit, an end plate turned the other side out,
/// or a curve turning the other way. A new end profile or curve direction redraws the module from the parameters it was
/// drawn from (<see cref="ModuleDrawing.AutoDraw"/>); what was done to it since, its identification and its section
/// joints, is kept. Joints keep their place relative to the main path, which runs along the centre of the end plates,
/// and are mirrored with a flipped curve. All of it works on the default drawing.
/// </summary>
public static class ModuleRedraw
{
    /// <summary>Whether the module can be redrawn: it was drawn from parameters.</summary>
    public static bool CanRedraw(ModuleFileContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return content.Module.Drawing.AutoDraw is not null;
    }

    /// <summary>Whether the module curves, so that its curve can be flipped.</summary>
    public static bool HasCurve(ModuleFileContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return content.Module.Drawing.AutoDraw?.Parameters is { } p && (p.Angle is > 0 || p.Radius is > 0);
    }

    /// <summary>Whether the plate at an exit with <paramref name="profile"/> can be turned: the profile is asymmetric.</summary>
    public static bool CanFlip(EndProfile? profile) => profile is not null && !EndProfileAnalysis.Analyze(profile).IsSymmetric;

    /// <summary>
    /// Whether redrawing moves the bends of section joints relative to the tracks: some joint is bent, and one of the
    /// end profiles, now or after the change, has its tracks off the centre of the plate. With centred tracks the bends
    /// keep their place.
    /// </summary>
    /// <param name="content">The module file content with the module to redraw.</param>
    /// <param name="newProfile">An end profile the module is about to get, if any.</param>
    public static bool BendsWillChange(ModuleFileContent content, EndProfile? newProfile = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        var drawing = content.Module.Drawing;
        if (!drawing.InternalJoints.Any(j => j.Bends is { Count: > 0 })) return false;
        var profiles = drawing.Exits.Select(e => content.FindEndProfile(e.EndProfile.Id)).Append(newProfile).OfType<EndProfile>();
        return profiles.Any(p => EndProfileAnalysis.AsymmetricElements(p).HasFlag(FitDifferences.TrackPositions));
    }

    /// <summary>
    /// The module with <paramref name="profile"/> at exit <paramref name="exitLabel"/>, redrawn; null if it can't be
    /// redrawn or doesn't have that exit. The exit's plate gets the orientation the drawing gives it; the others keep theirs.
    /// </summary>
    public static ModuleFileContent? WithEndProfile(ModuleFileContent content, string exitLabel, EndProfile profile, Catalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(catalogue);
        if (content.Module.Drawing.AutoDraw?.Parameters is not { } parameters || !parameters.Exits.Any(e => e.Label == exitLabel)) return null;
        var changed = parameters with
        {
            Exits = [.. parameters.Exits.Select(e => e.Label == exitLabel ? e with { EndProfile = profile.Designation, EndProfileId = profile.Id } : e)],
        };
        return Redraw(content, changed, catalogue.With(profile), mirrored: false, keepOrientation: label => label != exitLabel);
    }

    /// <summary>The module with its curve turning the other way, redrawn; null if it can't be redrawn or has no curve.</summary>
    public static ModuleFileContent? WithCurveFlipped(ModuleFileContent content, Catalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(catalogue);
        if (!HasCurve(content) || content.Module.Drawing.AutoDraw?.Parameters is not { } parameters) return null;
        var flipped = parameters with { CurveDirection = parameters.CurveDirection == CurveDirection.Left ? CurveDirection.Right : CurveDirection.Left };
        return Redraw(content, flipped, catalogue, mirrored: true, keepOrientation: _ => true);
    }

    /// <summary>
    /// The module with the plate at exit <paramref name="exitLabel"/> turned the other side out, and whether it turns an
    /// asymmetric end profile over (<see cref="Module.IsEndProfileSwitcher"/>) following; null without that exit.
    /// </summary>
    public static ModuleFileContent? WithEndFlipped(ModuleFileContent content, string exitLabel)
    {
        ArgumentNullException.ThrowIfNull(content);
        var drawing = content.Module.Drawing;
        if (drawing.Exit(exitLabel) is null) return null;
        var exits = drawing.Exits.Select(e => e.Label == exitLabel ? e.With(edited => edited.Orientation = Flipped(e.Orientation)) : e).ToList();
        return WithDrawing(content, drawing.With(edited => edited.Exits = exits), content.EndProfiles);
    }

    /// <summary>
    /// Whether a module with these exits turns an asymmetric end profile over: it has two exits, both asymmetric, with
    /// their plates the same side out. A plate at the far end faces the other way, so the same orientation puts the
    /// asymmetric feature on the other side of the track.
    /// </summary>
    public static bool IsEndProfileSwitcher(IReadOnlyList<ModuleExit> exits, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(exits);
        ArgumentNullException.ThrowIfNull(endProfiles);
        return exits is [var first, var second]
            && CanFlip(endProfiles(first.EndProfile.Id)) && CanFlip(endProfiles(second.EndProfile.Id))
            && first.Orientation == second.Orientation;
    }

    private static ProfileOrientation Flipped(ProfileOrientation orientation) =>
        orientation == ProfileOrientation.Normal ? ProfileOrientation.Mirrored : ProfileOrientation.Normal;

    private static ModuleFileContent? Redraw(ModuleFileContent content, AutoDrawParameters parameters, Catalogue catalogue, bool mirrored, Func<string, bool> keepOrientation)
    {
        var old = content.Module.Drawing;
        if (ModuleAutoDraw.Create(parameters with { ModuleId = content.Module.Id }, catalogue).Module is not { } drawn) return null;
        EndProfile? Lookup(Guid id) => catalogue.FindEndProfile(id) ?? content.FindEndProfile(id);
        var redrawn = drawn.Drawing;
        var (elements, tracks) = TracksOf(drawn, content.Module);
        var exits = redrawn.Exits
            .Select(e => keepOrientation(e.Label) && old.Exit(e.Label) is { } before ? e.With(edited => edited.Orientation = before.Orientation) : e)
            .ToList();
        var drawing = old.With(edited => { edited.Exits = exits; edited.MainPath = redrawn.MainPath; edited.Sides = redrawn.Sides; edited.Sections = redrawn.Sections; edited.InternalJoints = redrawn.InternalJoints; edited.TrackElements = [.. elements]; edited.OperatorSpaces = redrawn.OperatorSpaces; edited.PublicSides = redrawn.PublicSides; edited.DuckUnderAreas = redrawn.DuckUnderAreas; edited.Labels = redrawn.Labels; edited.RegistryMeasures = redrawn.RegistryMeasures; edited.AutoDraw = redrawn.AutoDraw; });
        drawing = WithJointsFrom(old, drawing, mirrored, Lookup);
        var profiles = drawing.Exits.Select(e => e.EndProfile.Id).Distinct().Select(Lookup).OfType<EndProfile>().ToList();
        return WithDrawing(content, drawing, profiles, tracks);
    }

    /// <summary>
    /// The track elements of the <paramref name="redrawn"/> module and the list of tracks they belong to. The redrawn
    /// elements are generated in the same order, so each keeps the track of <paramref name="old"/>'s list that the
    /// element in its place was part of. A redrawn module can have more tracks than before, e.g. with a double-track
    /// end profile in place of a single-track one: each track of the further elements is added to the list, with the
    /// lowest id that no track has and the lowest number that no track is called. Tracks that have lost their elements
    /// are kept, with what the user has entered for them.
    /// </summary>
    private static (IReadOnlyList<TrackElement> Elements, IReadOnlyList<StationTrack> Tracks) TracksOf(Module redrawn, Module old)
    {
        var before = old.Drawing.TrackElements;
        var tracks = old.Tracks.ToList();
        var added = new Dictionary<int, int>();
        var elements = redrawn.Drawing.TrackElements.Select((element, index) =>
        {
            if (index < before.Count) return element.With(edited => edited.StationTrackId = before[index].StationTrackId);
            // Only an operation location has station tracks; the line of a plain module has none (decided 2026-11-21).
            if (old.OperationLocation is null || element is BufferStop) return element.With(edited => edited.StationTrackId = null);
            var key = element.StationTrackId ?? -(index + 1);
            if (!added.TryGetValue(key, out var newId))
            {
                newId = StationTrackList.FreeId(tracks);
                added.Add(key, newId);
                var designation = StationTrackList.FreeDesignation(tracks);
                var displayOrder = tracks.Select(t => t.DisplayOrder).DefaultIfEmpty().Max() + 1;
                var track = (element.StationTrackId is { } id ? redrawn.Tracks.FirstOrDefault(t => t.Id == id) : null) ?? StationTrackList.GeneratedTrack(newId);
                tracks.Add(track.With(t =>
                {
                    t.Id = newId;
                    t.Designation = designation;
                    t.DisplayOrder = displayOrder;
                }));
            }
            return element.With(edited => edited.StationTrackId = newId);
        }).ToList();
        return (elements, tracks.Count == old.Tracks.Count ? [.. old.Tracks] : tracks);
    }

    /// <summary>
    /// <paramref name="drawing"/> with the joints of <paramref name="old"/> moved onto it: each point keeps its distance
    /// along the main path and its offset from it, mirrored when <paramref name="mirrored"/>, and the ends are put on
    /// the new outline. A joint that no longer cuts the module is drawn straight across the track where it crossed it,
    /// or left out if even that doesn't fit.
    /// </summary>
    private static ModuleDrawing WithJointsFrom(ModuleDrawing old, ModuleDrawing drawing, bool mirrored, Func<Guid, EndProfile?> endProfiles)
    {
        if (old.InternalJoints.Count == 0 || old.MainPath?.ToPath() is not { } oldMain || drawing.MainPath?.ToPath() is not { } newMain) return drawing;
        SegmentPath? outline;
        try
        {
            outline = ModuleGeometry.Outline(drawing, endProfiles);
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            outline = null;
        }
        if (outline is null) return drawing;
        var accepted = new List<InternalJoint>();
        var result = ModuleSections.WithJoints(drawing, outline, []) ?? drawing;
        foreach (var joint in old.InternalJoints)
        {
            var points = joint.Points.Select(p => Moved(p, oldMain, newMain, mirrored)).ToList();
            points[0] = ModuleSections.OnEdge(outline, points[0]);
            points[^1] = ModuleSections.OnEdge(outline, points[^1]);
            var moved = joint.With(edited => { edited.Start = points[0]; edited.End = points[^1]; edited.Bends = points.Count > 2 ? [.. points.Skip(1).Take(points.Count - 2)] : null; });
            InternalJoint? straight = ModuleSections.AcrossAt(outline, drawing.MainPath!, ModuleSections.DistanceAlongTrack(joint, old.MainPath!));
            foreach (var candidate in new[] { moved, straight }.OfType<InternalJoint>())
            {
                if (ModuleSections.WithJoints(drawing, outline, [.. accepted, candidate]) is not { } cut) continue;
                accepted.Add(candidate);
                result = cut;
                break;
            }
        }
        return result;
    }

    private static Point Moved(Point point, SegmentPath oldMain, SegmentPath newMain, bool mirrored)
    {
        var projection = oldMain.Project(point);
        var offset = (point - projection.Pose.Position).Dot(projection.Pose.Left);
        var pose = newMain.PoseAt(Math.Clamp(projection.Distance, 0, newMain.Length));
        return pose.Position + pose.Left * (mirrored ? -offset : offset);
    }

    private static ModuleFileContent WithDrawing(ModuleFileContent content, ModuleDrawing drawing, IReadOnlyList<EndProfile> profiles, IReadOnlyList<StationTrack>? tracks = null)
    {
        EndProfile? Lookup(Guid id) => profiles.FirstOrDefault(p => p.Id == id) ?? content.FindEndProfile(id);
        var isSwitcher = IsEndProfileSwitcher(drawing.Exits, Lookup);
        if (drawing.AutoDraw is { } record) drawing = drawing.With(edited => edited.AutoDraw = record with { Parameters = record.Parameters with { IsEndProfileSwitcher = isSwitcher } });
        var module = content.Module.With(m =>
        {
            m.IsEndProfileSwitcher = isSwitcher;
            if (tracks is not null) m.Tracks = [.. tracks];
            m.Drawing = drawing;
        });
        try
        {
            return ModuleFileContent.Create(module, Lookup);
        }
        catch (KeyNotFoundException)
        {
            return content with { Module = module };
        }
    }
}
