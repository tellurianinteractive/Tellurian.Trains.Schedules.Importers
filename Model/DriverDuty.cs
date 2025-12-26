using System.Globalization;

namespace Tellurian.Trains.Schedules.Model;

public class DriverDuty(int id, string identity) : IEquatable<DriverDuty>
{
    public int Id { get; init; } = id;

    public string Identity { get; } = identity.TextOrException(nameof(identity));
    public Sessions Sessions { get; set; } = Sessions.All;

    public ICollection<TrainPart> Parts { get; } = [];

    public ICollection<Note> Notes { get; } = [];

    public Schedule Schedule { get; internal set; } = default!; // Set by Schedule.Add()

    public bool Equals(DriverDuty? other) => Identity.Equals(other?.Identity, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is DriverDuty other && Equals(other);
    public override int GetHashCode() => Identity.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public override string ToString() =>
        Parts.Count == 0 ? Identity :
        string.Format(CultureInfo.CurrentCulture,
            "{0}: {1} - {2}", Identity, Parts.First().Departure, Parts.Last().Arrival);

}

public static class DriverDutyExtensions
{
    extension(DriverDuty duty)
    {
        public Maybe<TrainPart> Add(TrainPart part)
        {
            duty = duty.ValueOrException(nameof(duty));
            part = part.ValueOrException(nameof(part));
            if (!duty.Parts.Contains(part))
            {
                if (part.IsOverlapping(duty.Parts)) return new Maybe<TrainPart>($"Part {part} overlaps existing parts in driver duty '{duty.Identity}'");
                part.Duty = duty;
                duty.Parts.Add(part);
            }
            return new Maybe<TrainPart>(part);
        }
    }
}
