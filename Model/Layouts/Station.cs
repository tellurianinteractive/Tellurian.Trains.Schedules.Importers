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
        /// The number of participants needed to operate the station: one for each staff post in each
        /// shift, and none while the station is not manned.
        /// </summary>
        public int ParticipantsNeeded =>
            station.IsManned ? station.StaffPosts.Count * Math.Clamp(station.ShiftCount, 1, 3) : 0;

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
/// Provides extension members for the staffing of a <see cref="Layout"/>.
/// </summary>
public static class LayoutStaffingExtensions
{
    extension(Layout layout)
    {
        /// <summary>The number of participants needed to operate every station of the layout.</summary>
        public int ParticipantsNeeded => layout.OperationLocations.OfType<Station>().Sum(station => station.ParticipantsNeeded);
    }
}
