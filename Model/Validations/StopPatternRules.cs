namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// The rule for a train category's <em>stop pattern</em>: the operating locations where trains of that
/// category stop on their way (<see cref="TrainCategory.StopLocationIds"/>).
/// </summary>
/// <remarks>
/// <para>
/// The pattern is a positive list. What it names is stopped at, what it does not name is run through, and
/// the route creator plans a new train's stops by it (see <c>Plan.Create</c>). It can only narrow: a
/// location it names is still only stopped at where the train can stop there at all — the location is not
/// signal controlled and it exchanges what the train carries (see <c>Train.CanStopAt</c>).
/// </para>
/// <para>
/// A category with an empty list has no pattern rather than a pattern of nowhere, so a plan made before
/// there were stop patterns, and a category the planner has not got to yet, behave exactly as they did:
/// a route stops wherever the category can exchange, and no stop is reported. A category whose trains run
/// non-stop is said so by naming only the locations they do stop at.
/// </para>
/// <para>
/// Nothing here touches a train. The pattern says where a <em>new</em> route is given stops; the trains
/// already planned keep the stops they have, and a stop outside the pattern is reported (see
/// <c>Train.CheckStopPattern</c>) so the planner can decide between the train and the pattern — which of
/// the two is wrong is not the model's to choose.
/// </para>
/// <para>
/// <c>DeduceStopPatterns</c> mutates and does not persist, so it obliges the caller to save (in the app,
/// <c>ScheduleState.SaveAndNotify()</c>). Reading a plan runs it on the way in.
/// </para>
/// </remarks>
public static class StopPatternRules
{
    extension(Plan plan)
    {
        /// <summary>
        /// Fills in the stop pattern of every category that has none, from the locations where its trains
        /// already stop. Returns the number of categories given a pattern, so a caller can save only when
        /// something actually moved. Idempotent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is what gives a plan made before there were stop patterns the pattern its trains have been
        /// running all along, and it is written so that no train it reads can be reported afterwards: the
        /// pattern is the locations the category's trains stop at, and the check asks only whether a stop
        /// is one of them.
        /// </para>
        /// <para>
        /// A category that already has a pattern is left alone, as is one whose trains stop nowhere — there
        /// is nothing to deduce from, and an empty pattern is no pattern. A shunting category is skipped
        /// altogether: its tasks do not travel, so they have no route for a pattern to shape and no
        /// intermediate stop for the check to look at.
        /// </para>
        /// <para>
        /// Run this after <see cref="StopRules.ApplyStopRules"/>, which sets the flags a call is a stop by.
        /// </para>
        /// </remarks>
        public int DeduceStopPatterns()
        {
            plan = plan.ValueOrException(nameof(plan));
            if (plan.Timetable is not { } timetable) return 0;
            var deduced = 0;
            foreach (var category in timetable.TrainCategories)
            {
                if (category.HasStopPattern) continue;
                if (plan.DeduceStopPattern(category)) deduced++;
            }
            return deduced;
        }

        /// <summary>
        /// Gives <paramref name="category"/> the stop pattern its trains already run: the locations where
        /// they stop, whatever pattern it had before. Returns whether it was given one, which it is not
        /// where the category is a shunting one or its trains stop nowhere.
        /// </summary>
        /// <remarks>
        /// This is the planner's own "fill in from the trains", so it replaces what is there. The
        /// whole-plan <see cref="DeduceStopPatterns"/> asks it only of the categories that have no
        /// pattern, and so never takes one away.
        /// </remarks>
        /// <param name="category">The category to give a pattern.</param>
        public bool DeduceStopPattern(TrainCategory category)
        {
            plan = plan.ValueOrException(nameof(plan));
            ArgumentNullException.ThrowIfNull(category);
            if (plan.Timetable is not { } timetable || category.IsShunting) return false;
            var ids = timetable.TrainsIn(category)
                .SelectMany(train => train.Calls)
                .Where(call => call.IsStop)
                .Select(call => call.OperationLocation.Id)
                .Distinct()
                .ToList();
            if (ids.Count == 0) return false;
            category.StopLocationIds = ids;
            return true;
        }
    }
}
