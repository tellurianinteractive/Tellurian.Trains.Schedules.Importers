using System.Collections;
using System.Resources;

namespace Tellurian.Trains.Schedules.Model;

public readonly struct Sessions
{
    public Sessions(Days days) => Flags = (ushort)((ushort)days | ((ushort)days) << 7);
    public Sessions(params int[] sessions) => Flags = sessions.ToFlags;
    internal ushort Flags { get; init; }
}

[Flags]
public enum Days
{
    None = 0,
    Monday = 1,
    Tuesday = 1 << 1,
    Wednesday = 1 << 2,
    Thursday = 1 << 3,
    Friday = 1 << 4,
    Saturday = 1 << 5,
    Sunday = 1 << 6,
}

internal static class CommonSessionPatterns
{
    public static ushort All = 0b______00_1111111_1111111;
    public static ushort Odd = 0b______00_0101010_1010101;
    public static ushort Even = 0b_____00_1010101_0101010;
    public static ushort Third1 = 0b___00_0100100_1001001;
    public static ushort Third2 = 0b___00_1001001_0010010;
    public static ushort Third3 = 0b___00_0010010_0100100;
    public static ushort OnDemand = 0b_01_0000000_0000000;
}

internal static class CommonDayPatterns
{
    public static ushort Daily = 0b_____00_1111111_1111111;
    public static ushort Monday = 0b____00_0000001_0000001;
    public static ushort Thuesday = 0b__00_0000010_0000010;
    public static ushort Wednesday = 0b_00_0000100_0000100;
    public static ushort Thursday = 0b__00_0001000_0001000;
    public static ushort Friday = 0b____00_0010000_0010000;
    public static ushort Saturday = 0b__00_0100000_0100000;
    public static ushort Sunday = 0b____00_1000000_1000000;
}

public static class SessionsExtensions
{

    extension(Sessions sessions)
    {
        public Sessions And(Sessions other) => new() { Flags = sessions.Flags.And(other.Flags) };
        public Sessions Or(Sessions other) => new() { Flags = sessions.Flags.Or(other.Flags) };
        public bool IsOnDemand => (sessions.Flags & CommonSessionPatterns.OnDemand) > 0;

        public byte[] Numbers =>
            [.. new BitArray([sessions.Flags])
            .Cast<bool>()
            .Select((b, i) => (x: b, y: (byte)(i)))
            .Take(14) // Only 14 bits are for sessions.
            .Where(t => t.x)
            .Select(t => (byte)(t.y + 1))];


        internal bool IsConsequtiveSessions
        {
            get
            {
                var numbers = sessions.Numbers;
                if (numbers.Length == 0) return false;
                for (var i = 1; i < numbers.Length; i++)
                {
                    if (numbers[i] != numbers[i - 1] + 1) return false;
                }
                return true;
            }
        }

        public string SessionsNumbers
        {
            get
            {
                var numbers = sessions.Numbers;
                return numbers.Length switch
                {
                    0 => "None",
                    14 => "All",
                    _ when sessions.IsConsequtiveSessions => $"{numbers.First()}-{numbers.Last()}",
                    _ => string.Join(" ", numbers.Select(n => n.ToString())),
                };
            }
        }

        internal bool IsConsequtiveDays
        {
            get
            {
                var days = sessions.Days;
                if (days.Length == 0) return false;
                for (var i = 1; i < days.Length; i++)
                {
                    if (days[i].DayNumber != days[i - 1].DayNumber + 1) return false;
                }
                return true;

            }
        }

        public string DaysResourceKey
        {
            get
            {
                var days = sessions.Days;
                return days.Length switch
                {
                    0 => Resources.Days.None,
                    7 => Resources.Days.Daily,
                    _ when sessions.IsConsequtiveDays => $"{days.First().Translated}-{days.Last().Translated}",
                    _ => string.Join(",", days.Select(s => s.Translated)),
                };
            }
        }

        public string OnDemand => sessions.IsOnDemand ? "OnDemand" : string.Empty;

        public static Sessions All => new() { Flags = CommonSessionPatterns.All };
    }

    extension(ushort flags)
    {
        internal ushort And(ushort other) => (ushort)(flags & other);
        internal ushort Or(ushort other) => (ushort)(flags | other);
    }

    extension(int[]? sessions)
    {
        internal ushort ToFlags
        {
            get
            {
                if (sessions is null) return 0;
                ushort flags = 0;
                for (var i = 0; i < sessions.Length; i++)
                {
                    flags |= (ushort)(1 << (sessions[i] - 1));
                }
                return flags;
            }
        }
    }
}

public static class DaysExtensions
{
    private static readonly ResourceManager ResourceManager = new(typeof(Resources.Days));

    extension(Sessions sessions)
    {
        public Days[] Days =>
    [.. new BitArray([sessions.Flags])
            .Cast<bool>()
            .Select((b, i) => (x: b, y: (byte)(i)))
            .Take(7) // Only 7 bits are for days.
            .Where(t => t.x)
            .Select(t => _MondayToSunday[t.y+1])];

    }

    extension(Days days)
    {
        internal int DayNumber =>
            _MondayToSunday.Single(kv => kv.Value == days).Key;

        internal string Translated =>
            ResourceManager.GetString(days.ToString()) ?? days.ToString();

    }

    private static readonly Dictionary<int, Days> _MondayToSunday = new()
    {
        {1, Days.Monday },
        {2, Days.Tuesday },
        {3, Days.Wednesday },
        {4, Days.Thursday },
        {5, Days.Friday },
        {6, Days.Saturday },
        {7, Days.Sunday },
    };
}
