using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System.Data;
using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

namespace Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

/// <summary>
/// Provides functionality to read schedule data from XLSX (Excel) files.
/// This provider uses the ExcelDataReader library to parse Excel files
/// and extract worksheet data.
/// </summary>
/// <param name="logger">Logger for recording import progress and errors.</param>
public sealed class XlsxDataSetProvider(ILogger logger) : IDataSetProvider
{
    private readonly ILogger Logger = logger;

    /// <summary>
    /// Extracts field values from a data row as a string array.
    /// </summary>
    /// <param name="row">The data row to extract values from.</param>
    /// <returns>An array of string values representing the row data.</returns>
    public static string[] GetRowData(DataRow row) => row.GetRowFields();

    /// <summary>
    /// Imports schedule data from an XLSX (Excel) stream.
    /// </summary>
    /// <param name="stream">The input stream containing the Excel file data.</param>
    /// <param name="configuration">Configuration specifying which worksheets to read.</param>
    /// <returns>
    /// A <see cref="DataSet"/> containing the imported data tables,
    /// filtered to include only the worksheets specified in the configuration.
    /// </returns>
    /// <exception cref="Exception">Thrown when an error occurs while reading the Excel file.</exception>
    public DataSet? ImportSchedule(Stream stream, DataSetConfiguration configuration)
    {
        var worksheets = configuration.Worksheets;
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet();
            if (worksheets.Length > 0)
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    if (!worksheets.Any(w => w.Equals(table.TableName, StringComparison.OrdinalIgnoreCase)))
                    {
                        dataSet.Tables.Remove(table);
                    }
                }
            }
            return dataSet;
        }
        catch (Exception ex)
        {
            if(Logger.IsEnabled(LogLevel.Error))
                Logger.LogError(ex, "Error when reading {file}.", configuration.Name);
            throw;
        }
    }


}
