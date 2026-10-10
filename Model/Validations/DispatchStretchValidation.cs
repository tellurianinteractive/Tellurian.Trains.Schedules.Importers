using Tellurian.Trains.Schedules.Model.Resources;
using Tellurian.Trains.Schedules.Model.Schedules;

namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Finds trains on the line between stations at the same time where they cannot be (rule L3). The line
/// is judged per dispatch stretch, because the dispatchers at its two ends are the ones who agree what may
/// be on it.
/// </summary>
/// <remarks>
/// <para>
/// A dispatch stretch is divided into <em>control sections</em> by its control points (see
/// <c>IsControlPoint</c>): the signal controlled locations inside it, such as block posts and crossing
/// loops. A section holds as many trains at once as it has tracks, whatever their direction. Unmanned
/// locations nobody controls do not divide anything. A location worked from a manned station — a
/// controlled junction, unmanned station or industrial area — is not inside a dispatch stretch at all,
/// but at its end (see <c>IsDispatchEndpoint</c>).
/// </para>
/// <para>
/// On single track, trains running in opposite directions can only meet where there is somewhere to
/// meet (see <c>AllowsMeets</c>). So a run of single-track sections between two such places is a
/// <em>meet-free zone</em>: trains may follow each other through it, one per section, but two trains
/// in opposite directions may never be in it at the same time.
/// </para>
/// <para>
/// A train occupies the sections it runs over from its departure to its arrival. Where it passes a
/// control point without a call there, nothing says when it passed, so it occupies every section between
/// its two calls for the whole time. While it stands at an uncontrolled location it still occupies the
/// section around it; standing at a control point it occupies none, and neither does a train locked in on
/// a lockable track (see <c>StationCall.IsLockedIn</c>). As everywhere else, trains only meet
/// on a common session, and a train arriving just as another departs is no conflict.
/// </para>
/// <para>
/// Track stretches that no dispatch stretch covers — all of them, where the layout has none recorded —
/// are judged one by one, as before (see <c>TrackStretch.GetConflictingTrains</c>).
/// </para>
/// </remarks>
internal static class DispatchStretchValidation
{
    extension(Timetable timetable)
    {
        internal IEnumerable<ValidationError> GetStretchConflicts()
        {
            var layout = timetable.Layout;
            var dispatchStretches = layout.DispatchStretches.Where(d => d.Stretches.Count > 0).ToList();
            var covered = dispatchStretches.SelectMany(d => d.Stretches).ToHashSet();
            var result = new List<ValidationError>();
            result.AddRange(layout.TrackStretches.Where(s => !covered.Contains(s)).SelectMany(s => s.GetConflictingTrains()).Distinct());
            result.AddRange(GetDispatchStretchConflicts(dispatchStretches, timetable.Trains));
            return result;
        }
    }

    private static List<ValidationError> GetDispatchStretchConflicts(IEnumerable<DispatchStretch> dispatchStretches, IEnumerable<Train> trains)
    {
        // Sections and zones are keyed by the track stretches they span, so one shared between dispatch
        // stretches (the line up to a controlled junction) is judged once, with all trains using it.
        var sections = new Dictionary<string, Area>();
        var zones = new Dictionary<string, Area>();
        var visits = new HashSet<(StationCall, StationCall)>();
        var trainList = trains.ToList();

        foreach (var dispatchStretch in dispatchStretches)
        {
            var line = Line.Of(dispatchStretch);
            if (line is null) continue;
            foreach (var train in trainList)
            {
                foreach (var visit in line.VisitsOf(train))
                {
                    if (!visits.Add((visit.First, visit.Last))) continue;
                    line.AddOccupancies(visit, sections, zones);
                }
            }
        }

        var result = new List<ValidationError>();
        var reported = new HashSet<(Visit, Visit)>();
        foreach (var section in sections.Values) result.AddRange(section.CapacityConflicts(reported));
        foreach (var zone in zones.Values) result.AddRange(zone.OpposingConflicts(reported));
        return result;
    }

    /// <summary>
    /// A dispatch stretch laid out as its locations in order, divided into control sections and
    /// meet-free zones.
    /// </summary>
    private sealed class Line
    {
        private readonly OperationLocation[] _locations;
        private readonly int[] _boundaries; // location indexes of the ends and the control points
        private readonly (string Key, int Capacity)[] _sections; // between consecutive boundaries
        private readonly (string Key, int First, int Last)[] _zones; // runs of single-track sections

        private Line(OperationLocation[] locations, int[] boundaries, (string, int)[] sections, (string, int, int)[] zones)
        {
            _locations = locations;
            _boundaries = boundaries;
            _sections = sections;
            _zones = zones;
        }

        public static Line? Of(DispatchStretch dispatchStretch)
        {
            var stretches = dispatchStretch.Stretches.ToArray();
            var locations = new OperationLocation[stretches.Length + 1];
            locations[0] = stretches[0].Start;
            for (var i = 0; i < stretches.Length; i++)
            {
                if (!stretches[i].Start.Equals(locations[i])) return null; // not contiguous: regenerate the dispatch stretches
                locations[i + 1] = stretches[i].End;
            }

            var boundaries = Enumerable.Range(0, locations.Length)
                .Where(i => i == 0 || i == locations.Length - 1 || locations[i].IsControlPoint)
                .ToArray();
            var sections = new (string, int)[boundaries.Length - 1];
            for (var s = 0; s < sections.Length; s++)
            {
                var spanned = stretches[boundaries[s]..boundaries[s + 1]];
                sections[s] = (KeyOf(spanned), spanned.Min(t => t.TracksCount));
            }

            var zones = new List<(string, int, int)>();
            for (var s = 0; s < sections.Length; s++)
            {
                if (sections[s].Item2 != 1) continue;
                var first = s;
                while (s + 1 < sections.Length && sections[s + 1].Item2 == 1 && !locations[boundaries[s + 1]].AllowsMeets) s++;
                zones.Add((KeyOf(stretches[boundaries[first]..boundaries[s + 1]]), first, s));
            }
            return new Line(locations, boundaries, sections, [.. zones]);
        }

        private static string KeyOf(IEnumerable<TrackStretch> stretches) =>
            string.Join('|', stretches.Select(t => $"{t.Start.Signature}>{t.End.Signature}").Order(StringComparer.OrdinalIgnoreCase));

        private int IndexOf(OperationLocation location) => Array.IndexOf(_locations, location);
        private bool IsEnd(int index) => index == 0 || index == _locations.Length - 1;

        /// <summary>
        /// The train's runs over this dispatch stretch: each from a call at one end (or where the train
        /// starts, inside the stretch) to a call at an end (or where it finishes, inside the stretch),
        /// calling only inside the stretch in between. A run that leaves the stretch somewhere else
        /// belongs to another dispatch stretch.
        /// </summary>
        public IEnumerable<Visit> VisitsOf(Train train)
        {
            var calls = train.CallsInRunOrder;
            for (var i = 0; i < calls.Count; i++)
            {
                var start = IndexOf(calls[i].OperationLocation);
                if (start < 0 || !(IsEnd(start) || i == 0)) continue;
                var j = i + 1;
                while (j < calls.Count && IndexOf(calls[j].OperationLocation) is var k && k > 0 && !IsEnd(k)) j++;
                var last = j < calls.Count && IsEnd(IndexOf(calls[j].OperationLocation)) ? j : j == calls.Count ? j - 1 : -1;
                if (last <= i) continue;
                yield return new Visit(train, [.. calls.Skip(i).Take(last - i + 1)]);
            }
        }

        public void AddOccupancies(Visit visit, Dictionary<string, Area> sections, Dictionary<string, Area> zones)
        {
            var calls = visit.Calls;
            var direction = 0;
            for (var c = 0; c + 1 < calls.Count; c++)
            {
                var from = calls[c];
                var to = calls[c + 1];
                var a = IndexOf(from.OperationLocation);
                var b = IndexOf(to.OperationLocation);
                if (a != b)
                {
                    direction = Math.Sign(b - a);
                    var (low, high) = (Math.Min(a, b), Math.Max(a, b));
                    var covered = Enumerable.Range(0, _sections.Length)
                        .Where(s => Math.Max(_boundaries[s], low) < Math.Min(_boundaries[s + 1], high))
                        .ToList();
                    var passing = new StretchPassing(visit.Train, from, to);
                    foreach (var s in covered) Add(sections, _sections[s].Key, _sections[s].Capacity, visit, passing, direction);
                    foreach (var zone in _zones.Where(z => covered.Any(s => s >= z.First && s <= z.Last)))
                        Add(zones, zone.Key, 1, visit, passing, direction);
                }

                // Standing at the call it has come to, unless that ends the run.
                if (c + 2 >= calls.Count) continue;
                var next = IndexOf(calls[c + 2].OperationLocation);
                var standingDirection = Math.Sign(next - b) == direction ? direction : 0; // 0: turns back here
                var standing = new StretchPassing(visit.Train, visit.First, visit.Last);
                // A train locked in on a siding is off the line, so it blocks nothing.
                if (to.IsLockedIn) continue;
                if (!_boundaries.Contains(b))
                {
                    var s = Array.FindLastIndex(_boundaries, x => x < b);
                    Add(sections, _sections[s].Key, _sections[s].Capacity, visit, standing, standingDirection, to.Arrival, to.Departure);
                    foreach (var zone in _zones.Where(z => s >= z.First && s <= z.Last))
                        Add(zones, zone.Key, 1, visit, standing, standingDirection, to.Arrival, to.Departure);
                }
                else if (!_locations[b].AllowsMeets)
                {
                    // A single-track control point inside a zone: nobody can pass the train standing there.
                    var s = Array.IndexOf(_boundaries, b);
                    foreach (var zone in _zones.Where(z => s > z.First && s <= z.Last))
                        Add(zones, zone.Key, 1, visit, standing, standingDirection, to.Arrival, to.Departure);
                }
            }
        }

        private static void Add(Dictionary<string, Area> areas, string key, int capacity, Visit visit, StretchPassing passing, int direction) =>
            Add(areas, key, capacity, visit, passing, direction, passing.Departure, passing.Arrival);

        private static void Add(Dictionary<string, Area> areas, string key, int capacity, Visit visit, StretchPassing passing, int direction, Time start, Time end)
        {
            if (end <= start) return;
            if (!areas.TryGetValue(key, out var area)) areas[key] = area = new Area(capacity);
            area.Occupancies.Add(new Occupancy(visit, passing, direction, start, end));
        }
    }

    private sealed class Visit(Train train, IReadOnlyList<StationCall> calls)
    {
        public Train Train { get; } = train;
        public IReadOnlyList<StationCall> Calls { get; } = calls;
        public StationCall First => Calls[0];
        public StationCall Last => Calls[^1];
    }

    /// <summary>
    /// A train's time in a section or zone. <see cref="Direction"/> is +1 or -1 along the dispatch
    /// stretch, or 0 while standing where it turns back.
    /// </summary>
    private sealed record Occupancy(Visit Visit, StretchPassing Passing, int Direction, Time Start, Time End)
    {
        // A time past midnight is stored on the next day, so it is the same moment on the clock as the same
        // time before it — but on the session that day belongs to. Moved d days, an occupancy counts only
        // for the train's sessions d later (see TrackOccupancyExtensions.SharesSession).
        public bool IsActiveAt(Time instant, int session) => TrackOccupancyExtensions.DayShifts.Any(d =>
            session + d is >= 1 and <= 14 && Visit.Train.Sessions.Includes(session + d) && Start.AddDays(d) <= instant && End.AddDays(d) > instant);

        public bool Overlaps(Occupancy other) => TrackOccupancyExtensions.DayShifts.Any(d =>
            TrackOccupancyExtensions.SharesSession(Visit.Train.Sessions, other.Visit.Train.Sessions, d) && Start < other.End.AddDays(d) && other.Start.AddDays(d) < End);
        public bool IsOpposing(Occupancy other) => Direction == 0 || other.Direction == 0 || Direction != other.Direction;
    }

    private sealed class Area(int capacity)
    {
        public int Capacity { get; } = capacity;
        public List<Occupancy> Occupancies { get; } = [];

        /// <summary>
        /// More trains in the section at once, on one common session, than it has tracks. Concurrency
        /// only rises when a train enters, so testing every entering instant catches every peak.
        /// </summary>
        public IEnumerable<ValidationError> CapacityConflicts(HashSet<(Visit, Visit)> reported)
        {
            var occupancies = Occupancies.OrderBy(o => o.Start.Value).ToArray();
            if (occupancies.Select(o => o.Visit).Distinct().Count() <= Capacity) yield break;

            foreach (var entering in occupancies)
            {
                for (var session = 1; session <= 14; session++)
                {
                    if (!entering.Visit.Train.Sessions.Includes(session)) continue;
                    var onSession = occupancies.Where(o => o.IsActiveAt(entering.Start, session)).DistinctBy(o => o.Visit).ToList();
                    if (onSession.Count <= Capacity) continue;

                    // Report the earliest other train in the section with the one that tips it over.
                    var other = onSession.FirstOrDefault(o => o.Visit.Train.Number != entering.Visit.Train.Number);
                    if (other is null) continue;
                    if (Report(reported, other, entering) is { } error) yield return error;
                }
            }
        }

        /// <summary>
        /// Two trains in opposite directions in a meet-free zone at once, on a common session.
        /// </summary>
        public IEnumerable<ValidationError> OpposingConflicts(HashSet<(Visit, Visit)> reported)
        {
            var occupancies = Occupancies.OrderBy(o => o.Start.Value).ToArray();
            for (var i = 0; i < occupancies.Length; i++)
            {
                for (var j = i + 1; j < occupancies.Length; j++)
                {
                    var (one, another) = (occupancies[i], occupancies[j]);
                    if (one.Visit == another.Visit || one.Visit.Train.Number == another.Visit.Train.Number) continue;
                    if (!one.Overlaps(another) || !one.IsOpposing(another)) continue;
                    if (Report(reported, one, another) is { } error) yield return error;
                }
            }
        }

        private static ValidationError? Report(HashSet<(Visit, Visit)> reported, Occupancy one, Occupancy another)
        {
            var (first, second) = one.Start <= another.Start ? (one, another) : (another, one);
            if (reported.Contains((second.Visit, first.Visit)) || !reported.Add((first.Visit, second.Visit))) return null;
            var message = Message.Information(Strings.TrainBetweenPassingOverlapsInTimeWithTrainBetweenPassing,
                first.Visit.Train.Identity, first.Passing.SpanText, second.Visit.Train.Identity, second.Passing.SpanText);
            return ValidationError.StretchConflict(first.Passing.From.Track, first.Passing.To.Track, first.Passing, second.Passing, message);
        }
    }
}
