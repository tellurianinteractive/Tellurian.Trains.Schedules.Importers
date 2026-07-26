namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

public abstract class DutyPage(int pageNumber, string? instructionsMarkdown = null)
{
    public int PageNumber { get; } = pageNumber;
    public string InstructionsMarkdown { get; } = instructionsMarkdown ?? string.Empty;
    public virtual bool IsBlank { get; } = false;
    public virtual bool IsFront { get; } = false;
    public virtual bool IsPart { get; } = false;
    public virtual bool IsInstructions { get; }
}

public sealed class DriverDutyPage : DutyPage
{
    private DriverDutyPage(int pageNumber) : base(pageNumber) { }
    private DriverDutyPage(int pageNumber, string? instructionsMarkdown) : base(pageNumber, instructionsMarkdown) { }
    private DriverDutyPage(int number, DriverDuty duty) : base(number) { Duty = duty; }
    private DriverDutyPage(int number, DriverDuty duty, DriverDutyPart part) : base(number) { Duty = duty; DutyParts.Add(part); }

    public DriverDuty? Duty { get; }
    public List<DriverDutyPart> DutyParts { get; } = [];
    public override bool IsBlank => Duty is null && DutyParts.Count == 0 && !IsInstructions;
    public override bool IsFront => Duty is not null && DutyParts.Count == 0;
    public override bool IsPart => DutyParts.Any() && Duty is not null;

    public static DriverDutyPage Blank(int number) => new(number);
    public static DriverDutyPage Front(int number, DriverDuty duty) => new(number, duty);
    public static DriverDutyPage Part(int number, DriverDuty duty, DriverDutyPart part) => new(number, duty, part);
    public static DriverDutyPage Instructions(int number, string? instructionsMarkdown) =>
        new(number, instructionsMarkdown);
}

