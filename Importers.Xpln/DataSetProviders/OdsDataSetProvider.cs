using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

/// <summary>
/// Provides functionality to read schedule data from ODS (OpenDocument Spreadsheet) files.
/// This provider streams the XML structure within the ODS ZIP archive with a forward-only
/// <see cref="XmlReader"/> (no DOM, no XPath) and extracts worksheet data including cell values
/// and background colors.
/// </summary>
/// <param name="logger">Logger for recording import progress and errors.</param>
public sealed class OdsDataSetProvider(ILogger<OdsDataSetProvider> logger) : IDataSetProvider
{
    private readonly ILogger Logger = logger;

    /// <summary>
    /// The column name used to store background color information extracted from cells.
    /// </summary>
    public const string BackgroundColorColumnName = "BackgroundColor";

    private const string TableNs = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    private const string OfficeNs = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    private const string StyleNs = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    private const string FoNs = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";

    /// <summary>
    /// Imports schedule data from an ODS (OpenDocument Spreadsheet) stream.
    /// </summary>
    /// <param name="inputStream">The input stream containing the ODS file data.</param>
    /// <param name="dataSetConfiguration">Configuration specifying which worksheets to read.</param>
    /// <returns>
    /// A <see cref="DataSet"/> containing the imported data tables with cell values and background colors,
    /// or throws an exception if the file cannot be read.
    /// </returns>
    /// <exception cref="Exception">Thrown when an error occurs while reading the ODS file.</exception>
    public DataSet? ImportSchedule(Stream inputStream, DataSetConfiguration dataSetConfiguration)
    {
        try
        {
            using var archive = new ZipArchive(inputStream);
            var entry = archive.GetEntry("content.xml") ?? throw new FileNotFoundException("content.xml");
            using var stream = entry.Open();
            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            using var reader = XmlReader.Create(stream, settings);

            var dataSet = new DataSet(dataSetConfiguration.Name);
            var styleColors = new Dictionary<string, string>();

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element) continue;
                if (reader.LocalName == "automatic-styles" && reader.NamespaceURI == OfficeNs)
                {
                    ReadStyleBackgroundColors(reader, styleColors);
                }
                else if (reader.LocalName == "table" && reader.NamespaceURI == TableNs)
                {
                    var table = ReadTable(reader, dataSetConfiguration, styleColors);
                    if (table is not null) dataSet.Tables.Add(table);
                }
            }
            return dataSet;
        }
        catch (Exception ex)
        {
            if (Logger.IsEnabled(LogLevel.Error))
                Logger.LogError(ex, "Error when reading {stream}.", inputStream.ToString());
            throw;
        }
    }

    private static void ReadStyleBackgroundColors(XmlReader outer, Dictionary<string, string> colors)
    {
        using var reader = outer.ReadSubtree();
        reader.Read(); // position on <office:automatic-styles>
        string? currentStyleName = null;
        var currentIsCellStyle = false;
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;
            if (reader.LocalName == "style" && reader.NamespaceURI == StyleNs)
            {
                currentStyleName = reader.GetAttribute("name", StyleNs);
                currentIsCellStyle = reader.GetAttribute("family", StyleNs) == "table-cell";
            }
            else if (reader.LocalName == "table-cell-properties" && reader.NamespaceURI == StyleNs)
            {
                if (currentIsCellStyle && currentStyleName is not null)
                {
                    var bgColor = reader.GetAttribute("background-color", FoNs);
                    if (bgColor is not null && !bgColor.Equals("transparent", StringComparison.OrdinalIgnoreCase))
                        colors[currentStyleName] = bgColor;
                }
            }
        }
    }

    private DataTable? ReadTable(XmlReader outer, DataSetConfiguration configuration, Dictionary<string, string> styleColors)
    {
        var name = outer.GetAttribute("name", TableNs);
        var worksheetConfiguration = configuration.WorksheetConfiguration(name);
        using var reader = outer.ReadSubtree();
        reader.Read(); // position on <table:table>
        if (worksheetConfiguration is null)
        {
            while (reader.Read()) { } // skip unconfigured worksheet
            return null;
        }
        if (Logger.IsEnabled(LogLevel.Information))
            Logger.LogInformation("Reading table {table}.", worksheetConfiguration.WorksheetName);

        var dataTable = new DataTable(name);
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;
            if (reader.LocalName == "table-row" && reader.NamespaceURI == TableNs)
            {
                bool keepReading;
                using (var rowReader = reader.ReadSubtree())
                {
                    rowReader.Read(); // position on <table:table-row>
                    keepReading = ReadRow(rowReader, dataTable, worksheetConfiguration, styleColors);
                }
                if (!keepReading) break;
            }
        }
        if (dataTable.Rows.Count == 0) // Just add an empty row with one column if no data was found.
        {
            dataTable.Rows.Add(dataTable.NewRow());
            dataTable.Columns.Add();
        }
        return dataTable;
    }

    private static bool ReadRow(XmlReader reader, DataTable dataTable, WorksheetConfiguration configuration, Dictionary<string, string> styleColors)
    {
        var rowsRepeated = reader.GetAttribute("number-rows-repeated", TableNs);
        var repeat = rowsRepeated is null ? 1 : Convert.ToInt32(rowsRepeated, CultureInfo.InvariantCulture);
        if (repeat > configuration.MaxRowRepetitions) return false;

        while (dataTable.Columns.Count <= configuration.MaxReadColumns + 1)
            dataTable.Columns.Add();

        var (cells, backgroundColor) = ReadRowCells(reader, configuration, styleColors);

        for (var i = 0; i < repeat; i++)
        {
            var row = dataTable.NewRow();
            for (var c = 0; c < configuration.MaxReadColumns; c++)
                if (cells[c] is not null) row[c] = cells[c];
            if (backgroundColor is not null) row[configuration.BackgroundColorColumIndex] = backgroundColor;
            if (row.GetRowFields().Any(f => f.HasValue)) dataTable.Rows.Add(row);
        }
        return true;
    }

    private static (string?[] cells, string? backgroundColor) ReadRowCells(XmlReader reader, WorksheetConfiguration configuration, Dictionary<string, string> styleColors)
    {
        var cells = new string?[configuration.MaxReadColumns];
        string? backgroundColor = null;
        var cellIndex = 0;
        var isFirstCell = true;

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;
            if (reader.LocalName != "table-cell" || reader.NamespaceURI != TableNs) continue;

            var styleName = isFirstCell ? reader.GetAttribute("style-name", TableNs) : null;
            var cellRepeated = reader.GetAttribute("number-columns-repeated", TableNs);
            var value = ReadCellValue(reader);

            if (isFirstCell)
            {
                if (styleName is not null && styleColors.TryGetValue(styleName, out var color))
                    backgroundColor = color;
                isFirstCell = false;
            }

            var repeat = cellRepeated is null ? 1 : Convert.ToInt32(cellRepeated, CultureInfo.InvariantCulture);
            for (var i = 0; i < repeat && cellIndex < configuration.MaxReadColumns; i++)
            {
                cells[cellIndex] = value;
                cellIndex++;
            }
            // Continue consuming the rest of the (subtree-bounded) row even once full,
            // so the reader is left positioned at the end of the row.
        }
        return (cells, backgroundColor);
    }

    private static string? ReadCellValue(XmlReader reader)
    {
        // A typed cell carries its value in the office:value attribute; otherwise use the element text.
        var officeValue = reader.GetAttribute("value", OfficeNs);
        if (reader.IsEmptyElement) return officeValue;

        var depth = reader.Depth;
        StringBuilder? text = null;
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth) break;
            if (reader.NodeType is XmlNodeType.Text or XmlNodeType.CDATA or XmlNodeType.SignificantWhitespace)
                (text ??= new StringBuilder()).Append(reader.Value);
        }
        if (officeValue is not null) return officeValue;
        return text is null || text.Length == 0 ? null : text.ToString();
    }
}
