using System.Data;
using System.Data.Odbc;
using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Access;

internal static class LayoutStations
{
    public static IDbCommand CreateSelectCommand(string layoutName) =>
       new OdbcCommand
       {
           CommandType = CommandType.Text,
           CommandText = $"SELECT LayoutStationId, FullName, Signature FROM LayoutStations WHERE LayoutName = '{layoutName}'"
       };

    public static OdbcCommand CreateInsertCommand(int layoutId, int stationId)
    {
        var result = new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = "INSERT INTO LayoutStation (Layout, Station) VALUES (@LayoutId, @StationId)"
        };
        result.Parameters.AddWithValue("@LayoutId", layoutId);
        result.Parameters.AddWithValue("@StationId", stationId);
        return result;
    }

    public static void RecordHandler(IDataRecord record, Layout layout)
    {
        var result = new OperationLocation(record.GetInt32(record.GetOrdinal("Id")), record.GetString(record.GetOrdinal("FullName")), record.GetString(record.GetOrdinal("Signature")));
        layout.Add(result);
    }
}

internal static class LayoutStationExtensions
{
}
