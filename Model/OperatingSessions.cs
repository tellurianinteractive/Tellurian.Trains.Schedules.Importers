namespace Tellurian.Trains.Schedules.Importers.Model;

public class OperatingSessions(int id, IEnumerable<int> sessionNumbers)
{
    public int Id { get; init; } = id;
    private readonly IList<int> _sessions = [.. sessionNumbers.Where(s => s is >= 1 and <= 14).Distinct()];
    /// <summary>
    /// Gets the collection of session identifiers associated with the current instance.
    /// Session numbers are in the range 1 to 14.
    /// </summary>
    public IEnumerable<int> Sessions => _sessions;
    public int Count => _sessions.Count;
    public bool IsEmpty => _sessions.Count == 0;
}

public static class OperatingSessionsExtensions
{
    extension(OperatingSessions operatingSessions)
    {
        public static OperatingSessions OnDemand => new(-1000, []);
        public static OperatingSessions AllSessions => new(-1001, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
        public static OperatingSessions OddSessions => new(-1002, [1, 3, 5, 7, 9, 11, 13]);
        public static OperatingSessions EvenSessions => new(-1003, [2, 4, 6, 8, 10, 12, 14]);

        public OperatingSessions Union(OperatingSessions other) =>
            new(-1, operatingSessions.Sessions.Union(other.Sessions));

        public string OperatingDaysResourceKey =>
            operatingSessions.Sessions.Count() switch
            {
                0 => "OnDemand",
                14 => "Daily",
                _ when operatingSessions.Sessions.IsConsequtiveSessions() => $"{operatingSessions.Sessions.Min().DayName}To{operatingSessions.Sessions.Max().DayName}",
                _ => $"{string.Join("_", operatingSessions.Sessions.Select(s => s.DayName))}"
            };
    }

    extension(IEnumerable<int> sessionNumbers)
    {
        internal bool IsConsequtiveSessions()
        {
            var ordered = sessionNumbers.OrderBy(s => s).ToArray();
            if (ordered.Length == 0) return false;
            for (var i = 1; i < ordered.Length; i++)
            {
                if (ordered[i] != ordered[i - 1] + 1) return false;
            }
            return true;
        }
    }

    extension(int sessionNumber)
    {
        internal string DayName =>
            sessionNumber switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                7 => "Sunday",
                8 => "Monday",
                9 => "Tuesday",
                10 => "Wednesday",
                11 => "Thursday",
                12 => "Friday",
                13 => "Saturday",
                14 => "Sunday",
                _ => ""
            };
    }
}
