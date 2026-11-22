namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The physical layout of a <see cref="Layout"/>: its modules placed with ids of the layout's, the lines that order
/// them, the joints, and the end profiles and standards they need.
/// </summary>
public static class LayoutLines
{
    extension(Layout layout)
    {
        /// <summary>The tolerances that decide whether two module ends fit: the layout's own, else the defaults.</summary>
        public FitTolerances EffectiveFitTolerances => layout.FitTolerances ?? FitTolerances.Default;

        /// <summary>The main lines: those that are no branch. Each, with its branches, is one connected group.</summary>
        public IEnumerable<Line> MainLines => layout.Lines.Where(l => !l.IsBranch);

        /// <summary>Every entry of every line.</summary>
        public IEnumerable<LineEntry> Entries => layout.Lines.SelectMany(l => l.Entries);

        /// <summary>The placed module with <paramref name="id"/>, if any.</summary>
        public LayoutModule? FindModule(int id) => layout.Modules.FirstOrDefault(m => m.Id == id);

        /// <summary>The line with <paramref name="id"/>, if any.</summary>
        public Line? FindLine(int id) => layout.Lines.FirstOrDefault(l => l.Id == id);

        /// <summary>The entry that has the module <paramref name="moduleId"/> in a line; a module is in at most one (HP-7).</summary>
        public LineEntry? FindEntry(int moduleId) => layout.Entries.FirstOrDefault(e => e.ModuleId == moduleId);

        /// <summary>The line that has the module <paramref name="moduleId"/>, if any.</summary>
        public Line? LineOf(int moduleId) => layout.Lines.FirstOrDefault(l => l.Entries.Any(e => e.ModuleId == moduleId));

        /// <summary>The ids of the modules that are in the lines more than once, which HP-7 doesn't allow.</summary>
        public IEnumerable<int> ModulesPlacedMoreThanOnce() =>
            layout.Entries.GroupBy(e => e.ModuleId).Where(g => g.Count() > 1).Select(g => g.Key);

        /// <summary>What the planner has decided for the joint between two exits, in either order, if anything.</summary>
        public Joint? FindJoint(ExitReference a, ExitReference b) => layout.Joints.FirstOrDefault(j => j.Connects(a, b));

        /// <summary>The end profile with <paramref name="id"/>, if any.</summary>
        public EndProfile? FindEndProfile(Guid id) => layout.EndProfiles.FirstOrDefault(p => p.Id == id);

        /// <summary>The standard with <paramref name="id"/>, if any.</summary>
        public ModuleStandard? FindStandard(Guid id) => layout.Standards.FirstOrDefault(s => s.Id == id);

        /// <summary>
        /// The connected group of <paramref name="mainLine"/>: the line itself first, then the branches that start from a
        /// module of the group, in the order of <see cref="Layout.Lines"/>, until no more do.
        /// </summary>
        public IReadOnlyList<Line> GroupOf(Line mainLine)
        {
            ArgumentNullException.ThrowIfNull(mainLine);
            var group = new List<Line> { mainLine };
            var modules = mainLine.Entries.Select(e => e.ModuleId).ToHashSet();
            var added = true;
            while (added)
            {
                added = false;
                foreach (var branch in layout.Lines.Where(l => l.StartAt is { } start && !group.Contains(l) && modules.Contains(start.ModuleId)).ToList())
                {
                    group.Add(branch);
                    modules.UnionWith(branch.Entries.Select(e => e.ModuleId));
                    added = true;
                }
            }
            return group;
        }

        /// <summary>Adds <paramref name="line"/>, giving it the next id unless it has one not in use; returns it.</summary>
        public Line Add(Line line)
        {
            ArgumentNullException.ThrowIfNull(line);
            if (line.Id <= 0 || layout.Lines.Any(l => l.Id == line.Id)) line.Id = Next(layout.Lines.Select(l => l.Id));
            layout.Lines.Add(line);
            return line;
        }

        /// <summary>
        /// Places a copy of <paramref name="module"/> in the layout, with ids of the layout's that are given once and kept
        /// in every save (<c>LayoutsLayerSurvey.md</c> 6.1, 6.3): the layout module gets the next free id; its operation
        /// location, if it is one, the next free location id, and joins <see cref="Layout.OperationLocations"/>; each of
        /// its tracks the next free track id of the layout, keeping its id in the drawing as
        /// <see cref="StationTrack.SourceId"/>, and the drawing's track elements refer to the new ids.
        /// <paramref name="module"/> itself is not changed. The end profiles and standard it needs are added with
        /// <c>Include</c>.
        /// </summary>
        /// <returns>The placed module, not yet in any line.</returns>
        public LayoutModule Place(Module module)
        {
            ArgumentNullException.ThrowIfNull(module);
            var copy = module.Copy();
            var nextTrackId = Next(layout.Modules.SelectMany(m => m.Module.Tracks)
                .Concat(layout.OperationLocations.SelectMany(l => l.Tracks))
                .Select(t => t.Id));
            var location = copy.OperationLocation;
            if (location is not null)
            {
                location.Id = Next(layout.OperationLocations.Select(l => l.Id));
                location.Layout = layout;
                location.LayoutId = layout.Id;
                layout.OperationLocations.Add(location);
            }
            var trackIds = new Dictionary<int, int>();
            foreach (var track in copy.Tracks)
            {
                track.SourceId ??= track.Id;
                trackIds.TryAdd(track.Id, nextTrackId);
                track.Id = nextTrackId++;
                if (location is null) continue;
                track.Station = location;
                track.StationId = location.Id;
            }
            foreach (var element in copy.Drawing.TrackElements)
                if (element.StationTrackId is int id && trackIds.TryGetValue(id, out var placedId)) element.StationTrackId = placedId;
            var placed = new LayoutModule { Id = Next(layout.Modules.Select(m => m.Id)), Module = copy };
            layout.Modules.Add(placed);
            return placed;
        }

        /// <summary>
        /// Takes copies of the lines and joints of <paramref name="source"/>, and its end profiles, standards, overrides
        /// and fit tolerances, in place of the layout's own; the modules are left to the caller. For a layout that takes a
        /// hall plan's layout, whose module ids it keeps.
        /// </summary>
        public void TakeLinesFrom(Layout source)
        {
            ArgumentNullException.ThrowIfNull(source);
            layout.Lines = [.. source.Lines.Select(l => l.Copy())];
            layout.Joints = [.. source.Joints.Select(j => j.Copy())];
            layout.EndProfiles = [.. source.EndProfiles];
            layout.Standards = [.. source.Standards];
            layout.Overrides = [.. source.Overrides];
            layout.FitTolerances = source.FitTolerances;
        }

        /// <summary>Adds <paramref name="endProfile"/> unless the layout has an end profile with its id; returns the one it has.</summary>
        public EndProfile Include(EndProfile endProfile)
        {
            ArgumentNullException.ThrowIfNull(endProfile);
            if (layout.FindEndProfile(endProfile.Id) is { } existing) return existing;
            layout.EndProfiles.Add(endProfile);
            return endProfile;
        }

        /// <summary>
        /// Adds <paramref name="standard"/> unless the layout has a standard with its id; returns the one it has. A standard
        /// without an id is not added, since nothing can refer to it.
        /// </summary>
        public ModuleStandard Include(ModuleStandard standard)
        {
            ArgumentNullException.ThrowIfNull(standard);
            if (standard.Id is not Guid id) return standard;
            if (layout.FindStandard(id) is { } existing) return existing;
            layout.Standards.Add(standard);
            return standard;
        }
    }

    /// <summary>The next id after the highest of <paramref name="ids"/>, from 1.</summary>
    private static int Next(IEnumerable<int> ids) => Math.Max(0, ids.DefaultIfEmpty(0).Max()) + 1;
}
