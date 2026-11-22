using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The stretches that follow from the lines of a <see cref="Layout"/> (<c>LayoutsLayerSurvey.md</c> 3.3 and 3.6,
/// <c>ModelHarmonisation.md</c> decision 15). Track stretches are worked out automatically when a hall plan is read;
/// timetable stretches are only suggested, for the planner to take or not.
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
        /// modules (<see cref="LineStretch.Speed"/>), for a stretch that is added or has none; when no module has a
        /// limit, the stretch has no speed, which the planner warns of.
        /// Stretches that the lines don't give are left as they are.
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
                if (!stretch.HasSpeed && found.Speed is int speed) stretch.Speed = speed;
                if (!stretch.IsDistanceEntered && found.Distance is double distance) stretch.Distance = distance;
                if (!stretch.IsTimeEntered) stretch.Time = TrackStretchTiming.RunningMinutes(stretch, layout.Settings.TimeAndSpeed);
            }
            return result;
        }

        /// <summary>
        /// A timetable stretch for each line, branch included, of the <see cref="Layout.TrackStretches"/> along it in
        /// order; a branch starts at the location it leaves from. They are not added: the planner chooses whether to,
        /// since timetable stretches may overlap and a long line may be divided into several. A line is divided where a
        /// track stretch is missing or runs against the line, since a timetable stretch is contiguous. Numbers and ids
        /// follow those in use. Run <see cref="UpdateTrackStretchesFromLines"/> first.
        /// </summary>
        public IReadOnlyList<TimetableStretch> SuggestedTimetableStretches()
        {
            var found = layout.StretchesFromLines().Stretches;
            var nextId = Next(layout.TimetableStretches.Select(s => s.Id));
            var nextNumber = Next(layout.TimetableStretches.Select(s => int.TryParse(s.Number, CultureInfo.InvariantCulture, out var n) ? n : 0));
            var suggestions = new List<TimetableStretch>();
            foreach (var line in layout.Lines)
            {
                var run = new List<TrackStretch>();
                foreach (var stretch in found.Where(s => s.LineId == line.Id).OrderBy(s => s.Position))
                {
                    var track = layout.TrackStretches.FirstOrDefault(t => t.Start.Id == stretch.Start.Id && t.End.Id == stretch.End.Id);
                    if (track is null || (run.Count > 0 && run[^1].End.Id != track.Start.Id)) Flush();
                    if (track is not null) run.Add(track);
                }
                Flush();

                void Flush()
                {
                    if (run.Count == 0) return;
                    suggestions.Add(new TimetableStretch(nextId++, (nextNumber++).ToString(CultureInfo.InvariantCulture), line.Name ?? string.Empty) { Stretches = run });
                    run = [];
                }
            }
            return suggestions;
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

    extension(TrackStretch stretch)
    {
        /// <summary>
        /// The distance in metres that the lines and drawings of its layout give (<see cref="LineStretch.Distance"/>,
        /// survey 3.5): derived, never stored, so it can't be out of step with the drawings. Null when the layout has no
        /// lines through the stretch's locations, or not the drawings to measure it. Works the whole layout out on each
        /// call; for many stretches, use <c>layout.StretchesFromLines()</c> once.
        /// </summary>
        public double? MeasuredDistance =>
            stretch.Layout?.StretchesFromLines().Stretches
                .FirstOrDefault(s => (s.Start.Id == stretch.StartId && s.End.Id == stretch.EndId) || (s.Start.Id == stretch.EndId && s.End.Id == stretch.StartId))
                ?.Distance;
    }

    /// <summary>The next id after the highest of <paramref name="ids"/>, from 1.</summary>
    private static int Next(IEnumerable<int> ids) => Math.Max(0, ids.DefaultIfEmpty(0).Max()) + 1;
}
