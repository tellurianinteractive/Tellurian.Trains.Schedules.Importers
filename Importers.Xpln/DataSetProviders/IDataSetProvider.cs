using System.Data;

namespace Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

/// <summary>
/// Provides an abstraction for reading spreadsheet data from different file formats.
/// Implementations of this interface handle the specific format details (ODS, XLSX, etc.)
/// and convert the data into a <see cref="DataSet"/> for further processing.
/// </summary>
public interface IDataSetProvider
{
    /// <summary>
    /// Imports schedule data from a spreadsheet stream.
    /// </summary>
    /// <param name="stream">The input stream containing the spreadsheet data.</param>
    /// <param name="configiration">Configuration specifying which worksheets to read and their column settings.</param>
    /// <returns>
    /// A <see cref="DataSet"/> containing the imported data tables, or <c>null</c> if the import fails.
    /// Each worksheet specified in the configuration becomes a <see cref="DataTable"/> in the result.
    /// </returns>
    DataSet? ImportSchedule(Stream stream, DataSetConfiguration configiration);
}
