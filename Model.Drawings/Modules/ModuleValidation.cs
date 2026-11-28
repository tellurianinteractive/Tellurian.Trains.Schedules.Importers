namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>
/// Checks how a module is identified (MD-2). A module is identified either the FREMO way, by the owner's prefix and a
/// module number (e.g. <c>SFjä 0012</c>), or by a name, as in other standards and in some countries also for FREMO modules.
/// So a number needs the prefix, and without a number the module needs a name.
/// The owners' names are optional; an upload to the Module Registry fills in what is missing.
/// </summary>
public static class ModuleValidation
{
    /// <summary>The lowest allowed FREMO module number.</summary>
    public const int MinFremoNumber = 1;
    /// <summary>The highest allowed FREMO module number.</summary>
    public const int MaxFremoNumber = 9999;

    /// <summary>Validates how <paramref name="module"/> is identified; empty when there are no problems.</summary>
    public static IReadOnlyList<ModuleProblem> Validate(Module module)
    {
        ArgumentNullException.ThrowIfNull(module);
        return Validate(module.Name, module.Prefix, module.Number);
    }

    /// <summary>Validates the identifying properties as entered, before a module exists.</summary>
    public static IReadOnlyList<ModuleProblem> Validate(string? name, string? prefix, int? number)
    {
        var problems = new List<ModuleProblem>();
        if (number is { } given)
        {
            if (given is < MinFremoNumber or > MaxFremoNumber) problems.Add(new(ModuleProblemCode.FremoNumberOutOfRange, nameof(Module.Number)));
            if (string.IsNullOrWhiteSpace(prefix)) problems.Add(new(ModuleProblemCode.PrefixMissing, nameof(Module.Prefix)));
        }
        else if (string.IsNullOrWhiteSpace(name))
        {
            problems.Add(new(ModuleProblemCode.NameMissing, nameof(Module.Name)));
        }
        return problems;
    }
}
