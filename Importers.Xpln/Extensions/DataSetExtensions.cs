using System.Data;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

internal static class DataSetExtensions
{
    extension(DataRow row)
    {
        public string[] GetRowFields()
        {
            var items = row.ItemArray;
            if (items is null) return [];
            return items.Select(i => i is null ? string.Empty : i.ToString()).ToArray()!;
        }
        public bool IsBlankRow() =>
            row.GetRowFields().AreAllEmpty;

        public string? BackgroundColor(int? columnIndex) =>
            columnIndex.HasValue && row.Table.Columns.Contains($"Column{columnIndex}")
                ? row[columnIndex.Value] as string
                : null;
    }

    extension(IEnumerable<string> fields)
    {
        //public bool IsEmptyFields() =>
        //    fields.All(i => i.IsEmpty);
    }
}
