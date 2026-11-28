namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The stretches that follow from the lines of a <see cref="Layout"/> (<c>LayoutsLayerSurvey.md</c> 3.3 and 3.6,
/// <c>ModelHarmonisation.md</c> decision 15). Track stretches are worked out automatically when a hall plan is read;
/// timetable stretches are suggested by the planner from the <see cref="Layout.StretchLines"/> kept here.
/// </summary>
public static class LineStretches
{
    extension(Layout layout)
    {
        /// <summary>
        /// The stretches between neighbouring locations that the lines give: which plain-line modules, the sides the
        /// locations are left from, the number of tracks, the shortest route along the drawn tracks, the measured distance and the speed.
        /// Each runs in the direction of its line. Nothing in the layout is changed.
        /// </summary>
        public LineStretchesResult StretchesFromLines() => new LineWalker(layout).Walk();

        /// <summary>
        /// Brings <see cref="Layout.TrackStretches"/> in line with <see cref="StretchesFromLines"/>. A stretch between
        /// the same two locations, in either direction, is updated and keeps its direction: its sides, modules and
        /// number of tracks always, its distance unless <see cref="TrackStretch.IsDistanceEntered"/>, and its time from
        /// distance and speed unless <see cref="TrackStretch.IsTimeEntered"/>. Others are added with the next free ids. The speed is the one worked out from the
        /// modules (<see cref="LineStretch.Speed"/>) unless <see cref="TrackStretch.IsSpeedEntered"/>; when no module has
        /// a limit, the stretch has no speed, which the planner warns of.
        /// Stretches that the lines don't give are left as they are. Every stretch the lines give keeps what they measured
        /// (<see cref="TrackStretch.MeasuredDistance"/>, <see cref="TrackStretch.MeasuredSpeed"/>), and the layout keeps
        /// the order of the locations along each line (<see cref="Layout.StretchLines"/>).
        /// </summary>
        /// <returns>What the lines gave, with the problems found.</returns>
        public LineStretchesResult UpdateTrackStretchesFromLines()
        {
            var result = layout.StretchesFromLines();
            var nextId = Next(layout.TrackStretches.Select(s => s.Id));
            foreach (var found in result.Stretches)
            {
                var stretch = layout.FindTrackStretch(found.Start, found.End);
                if (stretch is null)
                {
                    stretch = new TrackStretch(nextId++, found.Start, found.End, found.Distance ?? 0, found.TracksCount ?? 1) { Speed = 0 };
                    layout.TrackStretches.Add(stretch);
                }
                var isReversed = stretch.Start.Id != found.Start.Id;
                stretch.StartSide = isReversed ? found.EndSide : found.StartSide;
                stretch.EndSide = isReversed ? found.StartSide : found.EndSide;
                stretch.ModuleIds = isReversed ? [.. found.ModuleIds.Reverse()] : [.. found.ModuleIds];
                if (found.TracksCount is int tracks) stretch.TracksCount = tracks;
                stretch.MeasuredDistance = found.Distance;
                stretch.MeasuredSpeed = found.Speed;
                if (!stretch.IsSpeedEntered) stretch.Speed = found.Speed ?? 0;
                if (!stretch.IsDistanceEntered && found.Distance is double distance) stretch.Distance = distance;
                if (!stretch.IsTimeEntered) stretch.Time = TrackStretchTiming.RunningMinutes(stretch, layout.Settings.TimeAndSpeed);
            }
            layout.StretchLines = [.. StretchLinesOf(layout, result.Stretches)];
            return result;
        }

        /// <summary>The track stretch between <paramref name="a"/> and <paramref name="b"/>, in either direction, if any.</summary>
        public TrackStretch? FindTrackStretch(OperationLocation a, OperationLocation b)
        {
            ArgumentNullException.ThrowIfNull(a);
            ArgumentNullException.ThrowIfNull(b);
            return layout.TrackStretches.FirstOrDefault(s =>
                (s.Start.Id == a.Id && s.End.Id == b.Id) || (s.Start.Id == b.Id && s.End.Id == a.Id));
        }
    }

    /// <summary>
    /// The order of the locations along each line of <paramref name="layout"/>: the stretches the walk found on it, by
    /// where on the line they start, in the line's direction.
    /// </summary>
    private static IEnumerable<StretchLine> StretchLinesOf(Layout layout, IReadOnlyList<LineStretch> found) =>
        layout.Lines.Select(line => new StretchLine
        {
            Id = line.Id,
            Name = line.Name,
            Legs = [.. found.Where(s => s.LineId == line.Id).OrderBy(s => s.Position).Select(s => new LineLeg(s.Start.Id, s.End.Id))],
        });

    /// <summary>The next id after the highest of <paramref name="ids"/>, from 1.</summary>
    private static int Next(IEnumerable<int> ids) => Math.Max(0, ids.DefaultIfEmpty(0).Max()) + 1;
}
