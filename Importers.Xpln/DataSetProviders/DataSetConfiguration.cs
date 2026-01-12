namespace Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

/// <summary>
/// Configuration for importing a data set from a spreadsheet file.
/// Contains settings for which worksheets to read and how to process them.
/// </summary>
/// <param name="Name">The name of the data set configuration.</param>
public record DataSetConfiguration(string Name)
{
    private readonly List<WorksheetConfiguration> _WorksheetConfigurations = [];

    /// <summary>
    /// Gets the names of all configured worksheets.
    /// </summary>
    public string[] Worksheets =>
        _WorksheetConfigurations.Select(x => x.WorksheetName).ToArray();

    /// <summary>
    /// Adds a worksheet configuration if it does not already exist.
    /// </summary>
    /// <param name="configuration">The worksheet configuration to add.</param>
    public void Add(WorksheetConfiguration configuration)
    {
        if (!ContainsWorksheet(configuration.WorksheetName))
            _WorksheetConfigurations.Add(configuration);
    }

    private bool ContainsWorksheet(string? name) =>
        !string.IsNullOrEmpty(name) &&
        _WorksheetConfigurations.Any((Func<WorksheetConfiguration, bool>)(wc => wc.WorksheetName.Equals(name, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Gets the configuration for a specific worksheet by name.
    /// </summary>
    /// <param name="name">The name of the worksheet.</param>
    /// <returns>The worksheet configuration if found; otherwise, <c>null</c>.</returns>
    public WorksheetConfiguration? WorksheetConfiguration(string? name) =>
        string.IsNullOrEmpty(name) ? null :
        _WorksheetConfigurations.SingleOrDefault((Func<WorksheetConfiguration, bool>)(wc => wc.WorksheetName.Equals(name, StringComparison.OrdinalIgnoreCase)));

}

/// <summary>
/// Configuration settings for reading a single worksheet from a spreadsheet.
/// </summary>
/// <param name="WorksheetName">The name of the worksheet to read.</param>
/// <param name="MaxReadColumns">The maximum number of columns to read from the worksheet.</param>
public record WorksheetConfiguration(string WorksheetName, int MaxReadColumns)
{
    /// <summary>
    /// Gets or sets the maximum number of times a row can be repeated before reading stops.
    /// This is used to detect empty rows in ODS format where empty rows may be represented as repeated.
    /// </summary>
    public int MaxRowRepetitions { get; init; } = 10;

    /// <summary>
    /// Gets the column index where background color information is stored.
    /// This is one beyond the maximum read columns.
    /// </summary>
    public int BackgroundColorColumIndex => MaxReadColumns + 1;
};

/// <summary>
/// Extension methods for <see cref="DataSetConfiguration"/>.
/// </summary>
public static class DataSetConfigurationExtensions
{
    extension(DataSetConfiguration configuration)
    {
        /// <summary>
        /// Gets the background color column index for a specific worksheet.
        /// </summary>
        /// <param name="worksheetName">The name of the worksheet.</param>
        /// <returns>The column index for background color data, or <c>null</c> if the worksheet is not configured.</returns>
        public int? BackgroundColorColumIndex(string worksheetName)
        {
            var worksheetConfiguration = configuration.WorksheetConfiguration(worksheetName);
            if (worksheetConfiguration is null) return null;
            return worksheetConfiguration.BackgroundColorColumIndex;
        }
    }
}

