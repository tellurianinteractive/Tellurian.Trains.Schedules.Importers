using System.Data;

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
            row.GetRowFields().IsEmptyFields();

    }

    extension(IEnumerable<string> fields)
    {
        public bool IsEmptyFields() =>
            fields.All(i => i.IsEmpty());
    }
}
