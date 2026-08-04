using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Tests;

[TestClass]
public class OdsDataSetProviderTests
{
    [TestMethod]
    public void ReadsFile()
    {
        var path = Path.Combine("Test data", "Montan2023H0e.de-DE.ods");
        var target = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
        using var stream = File.OpenRead(path);
        var dataSet = target.ImportSchedule(stream, DataSetConfiguration());
        Assert.IsNotNull(dataSet);
        WriteDataSet(dataSet, path);
    }

    [TestMethod]
    public void QuotesTheRowNumberTheSpreadsheetShows()
    {
        // FREMODERN-2023-Final-1-1 separates its two lines with a blank row (Routes row 8). A blank row is
        // not carried into the table, so the row after it is the table's seventh but the file's ninth — and
        // it is the file's number a message must quote, or the reader opens the wrong row.
        var path = Path.Combine("Test data", "FREMODERN-2023-Final-1-1.da-DK.ods");
        var target = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
        using var stream = File.OpenRead(path);
        var dataSet = target.ImportSchedule(stream, DataSetConfiguration());
        Assert.IsNotNull(dataSet);

        var routes = dataSet.Tables["Routes"]!;
        Assert.AreEqual(1, routes.SheetRowAt(0).Number, "The first row read is the file's first, the header.");
        Assert.AreEqual(7, routes.SheetRowAt(6).Number, "Above the blank row, the two numbers still agree.");

        var afterTheBlank = routes.SheetRowAt(7);
        Assert.AreEqual(9, afterTheBlank.Number, "The eighth row read is the file's ninth, the blank one having been passed.");
        Assert.AreEqual("Routes", afterTheBlank.Worksheet, "A message names the worksheet by its tab.");
        Assert.AreEqual("Ing", routes.Rows[7][2], "The file's row 9 starts the second line at Ing.");
    }

    private static DataSetConfiguration DataSetConfiguration()
    {
        var result = new DataSetConfiguration("XplnDocument");
        result.Add(new WorksheetConfiguration("StationTrack", 8));
        result.Add(new WorksheetConfiguration("Routes", 11));
        result.Add(new WorksheetConfiguration("Trains", 11));
        return result;
    }

    private static void WriteDataSet(DataSet dataSet, string fileName)
    {
        foreach (DataTable table in dataSet.Tables)
        {
            using var file = File.OpenWrite($"{fileName}-{table.TableName}.txt");
            var writer = new StreamWriter(file);
            foreach (DataRow row in table.Rows)
            {
                foreach (var cell in row.ItemArray)
                {
                    writer.Write(cell);
                    writer.Write(";");
                }
                writer.WriteLine();
            }
            writer.Flush();
            file.Close();
        }
    }
}
