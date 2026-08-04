using System.Data;
using System.Globalization;

namespace Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

/// <summary>
/// Where in the imported file something was found: the worksheet by the name on its tab, and the row by
/// the number the spreadsheet shows beside it. Formats itself for a message, so it can be given wherever a
/// message takes the place of a fault.
/// </summary>
/// <param name="Worksheet">The worksheet name, as it appears on the tab.</param>
/// <param name="Number">The row number, as the spreadsheet shows it.</param>
internal readonly record struct SheetRow(string Worksheet, int Number)
{
    /// <inheritdoc/>
    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetRow, Worksheet, Number);
}

/// <summary>
/// Reads back the spreadsheet row numbers a data provider recorded for a table's rows.
/// </summary>
internal static class SheetRowExtensions
{
    private const string SheetRowsKey = "SheetRows";

    extension(DataTable table)
    {
        /// <summary>
        /// Records, for each row of the table in order, the row number the spreadsheet shows for it. A
        /// provider that drops blank rows must record these, or a row's position in the table is taken for
        /// its position in the file and every message past the first blank row names the wrong row.
        /// </summary>
        /// <param name="sheetRows">The spreadsheet row number of each row of the table, in order.</param>
        internal void SetSheetRows(IEnumerable<int> sheetRows) =>
            table.ExtendedProperties[SheetRowsKey] = sheetRows.ToArray();

        /// <summary>
        /// Where the row at <paramref name="rowIndex"/> sits in the file. Falls back to counting from one
        /// when the provider recorded nothing, which is right for a provider that keeps every row.
        /// </summary>
        /// <param name="rowIndex">The index of the row within <see cref="DataTable.Rows"/>.</param>
        internal SheetRow SheetRowAt(int rowIndex) =>
            new(table.TableName,
                table.ExtendedProperties[SheetRowsKey] is int[] sheetRows && rowIndex >= 0 && rowIndex < sheetRows.Length
                    ? sheetRows[rowIndex]
                    : rowIndex + 1);
    }
}
