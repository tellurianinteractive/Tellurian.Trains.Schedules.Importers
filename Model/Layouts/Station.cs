using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// 
/// </summary>
public static class StationExtensions
{
    extension(Station station)
    {
        /// <summary>
        /// Adds a <see cref="Region"/> if not already present.
        /// </summary>
        /// <param name="region"></param>
        public void Add(Region region)
        {
            if (station.Regions.Contains(region)) return;
            station.Regions.Add(region);
        }
    }
}


/// <summary>
/// Provides extension members for the staffing of the stations of a <see cref="Timetable"/>.
/// </summary>
public static class TimetableStaffingExtensions
{
    extension(Timetable timetable)
    {
        /// <summary>
        /// The number of participants needed to operate <paramref name="station"/>: one for each staff post in each
        /// shift, and none while the station is not manned.
        /// </summary>
        public int ParticipantsNeeded(Station station)
        {
            ArgumentNullException.ThrowIfNull(station);
            if (!station.IsManned) return 0;
            var planning = timetable.PlanningOf(station);
            return planning.StaffPosts.Count * Math.Clamp(planning.ShiftCount, 1, 3);
        }

        /// <summary>The number of participants needed to operate every station of the layout.</summary>
        public int ParticipantsNeeded() => timetable.Layout.OperationLocations.OfType<Station>().Sum(timetable.ParticipantsNeeded);
    }
}
