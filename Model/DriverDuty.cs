using System.Globalization;

namespace Tellurian.Trains.Schedules.Model;

public class DriverDuty : IEquatable<DriverDuty>
{
    // Private parameterless constructor for EF Core
    private DriverDuty()
    {
        Identity = string.Empty;
        Parts = [];
        Notes = [];
    }

    public DriverDuty(int id, string? identity)
    {
        Id = id;
        Identity = identity.HasValue ? identity : id.ToString();
        Parts = [];
        Notes = [];
    }

    public int Id { get; set; }
    public string Identity { get; set; }
    public Sessions Sessions { get; set; } = Sessions.All;
    public ICollection<TrainPart> Parts { get; set; }
    public ICollection<DriverDutyNote> Notes { get; set; }

    // FK property for EF Core - Company that performs this duty
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    // FK property for EF Core
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = default!;

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
                part.DutyId = duty.Id;
                duty.Parts.Add(part);
            }
            return new Maybe<TrainPart>(part);
        }
    }
}
