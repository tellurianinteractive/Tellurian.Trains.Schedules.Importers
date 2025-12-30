using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model;

public sealed class Timetable : IEquatable<Timetable>
{
    // FK property for EF Core
    public int LayoutId { get; set; }
    public Layout Layout { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Train> Trains { get; set; }

    // Private parameterless constructor for EF Core
    private Timetable()
    {
        Layout = default!;
        Trains = [];
    }

    public Timetable(string name, Layout layout)
    {
        Name = name;
        Layout = layout;
        LayoutId = layout.Id;
        Trains = [];
    }

    public bool Equals(Timetable? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is Timetable other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;
}

public static class TimetableExtensions
{
    public static int StartHour(this Timetable me) =>
        (me?.Trains.Select(t => t.Calls.Min(c => c.Arrival)).Min(tt => tt).Hours()) ?? 0;
    public static int EndHour(this Timetable me) =>
        (me?.Trains.Select(t => t.Calls.Max(c => c.Arrival)).Max(tt => tt).Hours() + 1) ?? 24;

    public static IEnumerable<OperationLocation> Stations(this Timetable me) =>
        me is null ? Array.Empty<OperationLocation>() : me.Layout.Stations;

    public static Maybe<Train> Train(this Timetable me, string externalId) =>
        new(me?.Trains.Where(t => t.ExternalId == externalId), $"Train with external id '{externalId}' not found.");

    public static Train Add(this Timetable timetable, Train train)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        train = train.ValueOrException(nameof(train));
        if (!timetable.Trains.Contains(train))
        {
            train.Timetable = timetable;
            train.TimetableId = timetable.Id;
            timetable.Trains.Add(train);
        }
        return train;
    }
}
