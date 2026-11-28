namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A problem found in the lines of a <see cref="Layout"/>, and where.</summary>
/// <param name="Code">What is wrong.</param>
/// <param name="ModuleId">The <see cref="PlacedModule.Id"/> of the module concerned, if one is.</param>
/// <param name="Exit">The exit of that module concerned, if one is.</param>
public sealed record LineProblem(LineProblemCode Code, int? ModuleId = null, ExitPlace? Exit = null);
