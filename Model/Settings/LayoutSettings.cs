namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// All configurable settings for a layout, grouped by purpose.
/// </summary>
public sealed class LayoutSettings
{
    /// <summary>Identity settings: the operating theme, the modelling scale and the default country.</summary>
    public IdentitySettings Identity { get; set; } = new();

    /// <summary>General settings: language, the session/day model and the operating time window.</summary>
    public GeneralSettings General { get; set; } = new();

    /// <summary>Graphical timetable view and print preferences.</summary>
    public GraphicTimetableSettings GraphicTimetable { get; set; } = new();

    /// <summary>Time calculation settings: fast clock, speed mapping and default station times.</summary>
    public TimeAndSpeedSettings TimeAndSpeed { get; set; } = new();

    /// <summary>Validation settings: which validations run and their thresholds.</summary>
    public ValidationSettings Validation { get; set; } = new();

    /// <summary>Settings for integration with external services.</summary>
    public IntegrationSettings Integration { get; set; } = new();
}
