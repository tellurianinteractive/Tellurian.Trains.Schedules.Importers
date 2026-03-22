using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>
/// Provides extension methods for the <see cref="OperationsPlanner"/> class to support train creation and scheduling operations.
/// </summary>
public static class OperationsPlannerExtensions
{
    extension(OperationsPlanner planner)
    {
        /// <summary>
        /// Creates a new train scheduled to travel from the specified origin to the specified destination, starting at
        /// the given time and accounting for the required preparation period.
        /// </summary>
        /// <param name="from">The origin location from which the train will depart.</param>
        /// <param name="to">The destination location to which the train will travel.</param>
        /// <param name="startTime">The scheduled departure time for the train from the origin location.</param>
        /// <param name="preparationTime">The amount of time required to prepare the train before departure. Must be a non-negative duration.</param>
        /// <returns>A Train object representing the scheduled journey from the origin to the destination, including timing and
        /// preparation time.</returns>
        /// <exception cref="NotImplementedException">The method is not yet implemented.</exception>
        public Train Create(OperationLocation from, OperationLocation to, Time startTime, TimeSpan preparationTime)
        {
            // TODO: Find shortest path for train in the Layout and calculate run times and stop times
            // using the TimetableSettings.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves the specified train forward or backward by the given number of minutes and returns the updated train instance.
        /// </summary>
        /// <param name="train">The train to move. Cannot be null.</param>
        /// <param name="minutes">The number of minutes to move the train forward och backwards in time.</param>
        /// <returns>A new or updated Train instance representing the state of the train after moving forward or backwards by the specified
        /// number of minutes.</returns>
        /// <exception cref="NotImplementedException">The method is not implemented.</exception>
        public Train Move(Train train, int minutes)
        {
            throw new NotImplementedException();
        }

    }
}
