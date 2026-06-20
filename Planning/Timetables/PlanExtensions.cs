namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>
/// Provides extension methods for the <see cref="Plan"/> class to support train creation and scheduling operations.
/// </summary>
public static class PlanExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Creates a new train scheduled to travel from the specified origin to the specified destination, starting at
        /// the given time and accounting for the required preparation period.
        /// </summary>
        /// <param name="from">The origin location from which the train will depart.</param>
        /// <param name="to">The destination location to which the train will travel.</param>
        /// <param name="startTime">The scheduled departure time for the train from the origin location.</param>
        /// <param name="preparationMinutes">The number of minutes required to prepare the train before first departure. Must be a non-negative value.</param>
        /// <param name="finishingMinutes">The number of minutes required to finish the train aftler last arrival. Must be a non-negative value.</param>
        /// <returns>A Train object representing the scheduled journey from the origin to the destination, including timing and
        /// preparation time.</returns>
        /// <exception cref="NotImplementedException">The method is not yet implemented.</exception>
        public Train Create(OperationLocation from, OperationLocation to, Time startTime, int preparationMinutes = 10, int finishingMinutes = 10)
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
        /// <returns>An updated train instance representing the state of the train after moving forward or backwards by the specified
        /// number of minutes.</returns>
        /// <exception cref="NotImplementedException">The method is not implemented.</exception>
        public Train Move(Train train, int minutes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clones the train and creates a copy with timings moved the specified number of minutes
        /// </summary>
        /// <param name="train">The train to clone</param>
        /// <param name="minutes">The number of minutes to move the train forward och backwards in time.</param>
        /// <returns>A new train that is the clone of the original train.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>A train must start between 00:00 and 23:59. The operation should fail if the train's start time falls out of bounds</remarks>
        public Train Clone(Train train, int minutes)
        {
            throw new NotImplementedException();
        }

    }
}
