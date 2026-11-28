using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The timetable stretches the planner can suggest from the lines of the hall plan a layout was made in
/// (<c>ModelHarmonisation.md</c> decision 15). Timetable stretches are the planner's own: made by hand, or taken from
/// these suggestions (<c>ModelSurfaceAnalysis.md</c> 3.3).
/// </summary>
public static class TimetableStretchSuggestions
{
    extension(Layout layout)
    {
        /// <summary>Whether the layout has lines to suggest timetable stretches from.</summary>
        public bool CanSuggestTimetableStretches => layout.StretchLines.Any(l => l.Legs.Count > 0);

        /// <summary>
        /// A timetable stretch for each line, branch included, of the <see cref="Layout.TrackStretches"/> along it in
        /// order (<see cref="Layout.StretchLines"/>); a branch starts at the location it leaves from. They are not
        /// added: the planner chooses whether to, since timetable stretches may overlap and a long line may be divided
        /// into several. A line is divided where a track stretch is missing, and where the track stretches turn to run
        /// against the line: those that do are a timetable stretch of their own, in their own direction. So a loop that
        /// the planner has divided at a station, turning the stretches of one half to end there, gives two timetable
        /// stretches that both end at that station. Numbers and ids follow those in use.
        /// </summary>
        public IReadOnlyList<TimetableStretch> SuggestedTimetableStretches()
        {
            var nextId = Next(layout.TimetableStretches.Select(s => s.Id));
            var nextNumber = Next(layout.TimetableStretches.Select(s => int.TryParse(s.Number, CultureInfo.InvariantCulture, out var n) ? n : 0));
            var suggestions = new List<TimetableStretch>();
            foreach (var line in layout.StretchLines)
            {
                var run = new List<TrackStretch>();
                var isRunForward = true;
                foreach (var leg in line.Legs)
                {
                    if (layout.TrackStretches.FirstOrDefault(s => leg.Joins(s.StartId, s.EndId)) is not { } track)
                    {
                        Flush();
                        continue;
                    }
                    var isForward = track.StartId == leg.StartId;
                    var continues = run.Count > 0 && isForward == isRunForward &&
                        (isForward ? run[^1].EndId == track.StartId : run[^1].StartId == track.EndId);
                    if (!continues) Flush();
                    run.Add(track);
                    isRunForward = isForward;
                }
                Flush();

                void Flush()
                {
                    if (run.Count == 0) return;
                    if (!isRunForward) run.Reverse();
                    suggestions.Add(new TimetableStretch(nextId++, (nextNumber++).ToString(CultureInfo.InvariantCulture), line.Name ?? string.Empty) { Stretches = run });
                    run = [];
                }
            }
            return suggestions;
        }
    }

    /// <summary>The next id after the highest of <paramref name="ids"/>, from 1.</summary>
    private static int Next(IEnumerable<int> ids) => Math.Max(0, ids.DefaultIfEmpty(0).Max()) + 1;
}
