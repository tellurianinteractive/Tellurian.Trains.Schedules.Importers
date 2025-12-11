using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;

namespace Tellurian.Trains.Schedules.Importers.Model;

[DataContract(IsReference = true)]
public class DriverDuty : IEquatable<DriverDuty>
{
    public DriverDuty(string identity)
    {
        Identity = identity.TextOrException(nameof(identity));
        Parts = [];
        Notes = [];
        Schedule = default!; // Set by Schedule.Add()
    }

    [DataMember(IsRequired = false, Order = 1, Name = "Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#pragma warning disable CS0649 // Field is assigned by EF/deserialization
    private int _Id;
#pragma warning restore CS0649

    public int Id => _Id;

    [DataMember(IsRequired = true, Order = 2)]
    public string Identity { get; }

    [DataMember(IsRequired = true, Order = 3)]
    public ICollection<TrainPart> Parts { get; }

    [DataMember(IsRequired = true, Order = 4)]
    public ICollection<Note> Notes { get; }

    public Schedule Schedule { get; internal set; }

    public bool Equals(DriverDuty? other) => Identity.Equals(other?.Identity, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is DriverDuty other && Equals(other);
    public override int GetHashCode() => Identity.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public override string ToString() =>
        Parts.Count == 0 ? Identity :
        string.Format(CultureInfo.CurrentCulture,
            "{0}: {1} - {2}", Identity, Parts.First().Departure, Parts.Last().Arrival);

#pragma warning disable CS8618 // Properties initialized by EF/deserialization
    private DriverDuty() { } // Required for deserialization and EF.
#pragma warning restore CS8618
}

public static class DriverDutyExtensions
{
    public static Maybe<TrainPart> Add(this DriverDuty duty, TrainPart part)
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
