using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

/// <summary>
/// Maps the persisted, layout-level <see cref="LayoutSettings"/> to the rendering-level
/// <see cref="GraphSettings"/> used by the graphical schedule. Derived layout values
/// (axis label spacing, margins) are taken from <see cref="GraphSettings.Default"/>;
/// only the user-controlled preferences and the operating time window are overridden.
/// </summary>
public static class GraphSettingsMapping
{
    extension(LayoutSettings settings)
    {
        /// <summary>Builds a <see cref="GraphSettings"/> from these layout settings.</summary>
        public GraphSettings ToGraphSettings()
        {
            var gt = settings.GraphicTimetable;
            return GraphSettings.Default with
            {
                AxisDirection = gt.Orientation == TimeAxisOrientation.Vertical
                    ? TimeAxisDirection.Vertical
                    : TimeAxisDirection.Horisontal,
                DefaultStartTime = settings.General.StartTime,
                DefaultEndTime = settings.General.EndTime,
                BreakTime = settings.General.BreakTime,
                MinuteSpacing = gt.MinuteSpacing,
                KilometerSpacing = gt.KilometerSpacing,
                MinStationSpacing = gt.StationSpacing,
                TrackSpacing = gt.TrackSpacing,
                ShowArrivalMinutes = gt.ShowArrivalMinutes,
                ShowDepartureMinutes = gt.ShowDepartureMinutes,
                ShowCompany = gt.ShowCompany,
                ShowTrainCategory = gt.ShowTrainCategory,
            };
        }
    }
}
