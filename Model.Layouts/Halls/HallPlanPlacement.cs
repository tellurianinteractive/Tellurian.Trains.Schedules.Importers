namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>
/// Where the modules of a hall plan stand and what that shows (<c>LayoutsLayerSurvey.md</c> 2.8): the placement of each
/// module, the closure gap of each loop or rejoining line, each joint's fit and height step, and the problems found.
/// </summary>
/// <param name="Modules">Every module that could be placed.</param>
/// <param name="Closures">Every <see cref="Line.EndAt"/> joint whose two modules are placed.</param>
/// <param name="Joints">Every joint of the lines.</param>
/// <param name="Problems">Each problem once: modules not placed, exits not drawn, closure gaps and height steps over the hall plan's tolerances, ends that don't fit.</param>
public sealed record HallPlanPlacement(
    IReadOnlyList<ModulePlacement> Modules,
    IReadOnlyList<ClosureCheck> Closures,
    IReadOnlyList<JointCheck> Joints,
    IReadOnlyList<LineProblem> Problems)
{
    /// <summary>The placement of the module with <paramref name="moduleId"/>, if it could be placed.</summary>
    public ModulePlacement? Find(int moduleId) => Modules.FirstOrDefault(m => m.ModuleId == moduleId);
}
