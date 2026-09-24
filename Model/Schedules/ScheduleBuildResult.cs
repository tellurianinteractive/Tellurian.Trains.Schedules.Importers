namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Reports what an automatic build did: how much work it could give to the schedules the plan already
/// had, and what it had to create for the trains that fitted none of them.
/// </summary>
/// <param name="Created">The schedules created, in build order.</param>
/// <param name="Extended">The schedules already in the plan that gained at least one part, in the
/// order they were extended.</param>
/// <param name="AddedParts">The number of train parts worked into schedules that already existed.</param>
/// <seealso cref="PlanScheduleBuilderExtensions.BuildSchedulesAutomatically"/>
public sealed record ScheduleBuildResult(
    IReadOnlyList<Schedule> Created,
    IReadOnlyList<Schedule> Extended,
    int AddedParts);
