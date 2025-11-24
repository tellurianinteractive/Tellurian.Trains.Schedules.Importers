using System.Data.Odbc;
using System.Diagnostics;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Xpln.Tests;
internal static class ScheduleExtensions
{
    public static void SaveToDatabase(this Schedule me, string targetconnectionString)
    {
        var layoutId = me.GetOrCreateLayout(targetconnectionString);
        //me.SaveTrains(layoutId, targetconnectionString);
        me.SaveLocos(layoutId, targetconnectionString);
    }

    private static int GetOrCreateLayout(this Schedule me, string targetconnectionString)
    {
        var id = GetLayoutId(targetconnectionString);
        if (id > 0) return id;

        var sql = $"""
            INSERT INTO [Layout] 
            ([Name], [StartWeekday], [StartHour], [BreakHour], [EndHour], [IsCurrent], [TrainCategoryYear], [TrainCategoryCountry], [FontFamily])
            VALUES
            ('{me.Name}', 7, 5, 14, 23, 1, 2001, 3, 'SJ Sans' )
            """;
        using var connection = new OdbcConnection(targetconnectionString);
        connection.Open();
        var saveCommand = new OdbcCommand(sql) { Connection = connection };
        var result = saveCommand.ExecuteNonQuery();
        return GetLayoutId(targetconnectionString);
    }

    private static int GetLayoutId(string targetconnectionString)
    {
        using var connection = new OdbcConnection(targetconnectionString);
        connection.Open();
        var sql = "SELECT [Id] FROM [Layout] WHERE [IsCurrent] <> 0";
        var getCommand = new OdbcCommand(sql) { Connection = connection };
        var id = getCommand.ExecuteScalar();
        return id is null ? 0 : (int)id;
    }



    private static void SaveTrains(this Schedule me, int layoutId, string connectionString)
    {
        using var connection = new OdbcConnection(connectionString);
        var stations = GetStations(layoutId, connectionString);
        var tracks = stations.SelectMany(s => s.Tracks).ToArray();
        connection.Open();
        foreach (var train in me.Timetable.Trains)
        {
            var trainSql = $"INSERT INTO [Train] ([Layout], [Operator], [Number], [OperatingDays], [Category]) VALUES ({layoutId}, 'DSB', {train.Number}, 8, 1)";
            var saveTrainsCommand = new OdbcCommand(trainSql) { Connection = connection };
            saveTrainsCommand.ExecuteNonQuery();
            var getTrainCommand = new OdbcCommand($"SELECT Id FROM TRAIN WHERE Layout = {layoutId} AND Number = {train.Number} ") { Connection = connection };
            var trainId = (int?)getTrainCommand.ExecuteScalar();
            if (train.Number == "30") Debugger.Break();
            try
            {
                if (trainId.HasValue)
                {
                    int callNumber = 0;
                    var callsCount = train.Calls.Count;
                    foreach (var call in train.Calls)
                    {
                        callNumber++;
                        var station = stations.Single(s => s.Signature.Equals(call.Station.Signature, StringComparison.OrdinalIgnoreCase));
                        if (station is not null)
                        {
                            var track = station.Tracks.SingleOrDefault(t => t.Number == call.Track.Number);
                            if (track is not null)
                            {
                                var callSql = "INSERT INTO TrainStationCall (IsTrain, IsStationTrack, ArrivalTime, DepartureTime, IsStop, HideArrival, HideDeparture) VALUES " +
                                    $"({trainId}, {track.Id}, '{call.Arrival}', '{call.Departure}', -1, {(callNumber == 1 ? -1 : 0)}, {(callNumber == callsCount ? -1 : 0)} )";
                                var saveCallCommand = new OdbcCommand(callSql) { Connection = connection };
                                saveCallCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Debugger.Break();
            }
        }
    }

    private static void SaveLocos(this Schedule me, int layoutId, string connectionString)
    {
        using var connection = new OdbcConnection(connectionString);
        var stations = GetStations(layoutId, connectionString);
        var tracks = stations.SelectMany(s => s.Tracks).ToArray();
        connection.Open();
        const int daily = 8;

        var count = 0;
        var scheduleNumber = 1;
        foreach (var loco in me.LocoSchedules)
        {
            var t = loco.Number.Split(' ');
            
            var locoOperator = t.Length > 0 ? t[0] : string.Empty;
            var locoNumber = t.Length > 1 ? int.Parse(t[^1]) : 0;
            var locoClass = t.Length > 2 ? t[1] : string.Empty;
            var homeStationId = 641;

            var sql1 = $"""
                    INSERT INTO [LocoSchedule] ([Layout], [Number], [ExternalKey])
                    VALUES ({layoutId}, {scheduleNumber}, '{loco.Number}') 
                    """;
            var saveScheduleCommand = new OdbcCommand(sql1, connection);
            count = saveScheduleCommand.ExecuteNonQuery();

            var sql2 = $"SELECT [Id] FROM [LocoSchedule] WHERE [Layout] = {layoutId} AND [ExternalKey] = '{loco.Number}'" ;
            var getScheduleCommand = new OdbcCommand(sql2, connection);
            int scheduleId = (int?)getScheduleCommand.ExecuteScalar() ?? 0;

            foreach (var part in loco.Parts)
            {
                var sql31 = $"SELECT [CallId] FROM [StationDepartures] WHERE [Number] = {part.Train.Number} AND [Signature] = '{part.From.Station.Signature}'";
                var sql32 = $"SELECT [CallId] FROM [StationArrivals] WHERE [Number] = {part.Train.Number} AND [Signature] = '{part.To.Station.Signature}'";
                var fromCommand = new OdbcCommand(sql31, connection);
                var toCommand = new OdbcCommand(sql32, connection);
                var fromCallId = (int?)fromCommand.ExecuteScalar() ?? 0;
                var toCallId = (int?)toCommand.ExecuteScalar() ?? 0;
                if (fromCallId == 0 || toCallId == 0) throw new ArgumentOutOfRangeException($"From {part.From.Station.Signature} fromCallId = {fromCallId} - To {part.To.Station.Signature} toCallId = {toCallId}");
                var sql3 = $"""
                    INSERT INTO [LocoScheduleTrain] ([LocoSchedule], [FromDeparture], [ToArrival], [FromParking], [ToParking], [ReverseLoco], [UseNote])
                    VALUES ({scheduleId}, {fromCallId}, {toCallId}, -1, -1, -1, -1)
                    """;
                var partCommand = new OdbcCommand(sql3, connection);
                count = partCommand.ExecuteNonQuery();
            }

            var sql4 = $"""
                INSERT INTO [Loco] ([Layout], [Number], [Operator], [Class], [HomeStation], [Note], [ExternalKey])
                VALUES ({layoutId}, {locoNumber}, '{locoOperator}', '{locoClass}', {homeStationId}, '', '{loco.Number}' )
                """;
            var saveLocoCommand = new OdbcCommand(sql4, connection);
            count = saveLocoCommand.ExecuteNonQuery();

            var sql5 = $"SELECT [Id] FROM [Loco] WHERE [Layout] = {layoutId} AND [ExternalKey] = '{loco.Number}'";
            var getLocoCommand = new OdbcCommand(sql5, connection);
            var locoId = (int?)getLocoCommand.ExecuteScalar() ?? 0;

            var sql6 = $"""
                INSERT INTO [LocoScheduleOperatingDays] ([Loco], [OperatingDays], [LocoSchedule], [IsFirstDay])
                VALUES ({locoId}, {daily}, {scheduleId}, -1)
                """;
            var saveLocoScheduleOperatingDaysCommand = new OdbcCommand(sql6, connection);
            count = saveLocoScheduleOperatingDaysCommand.ExecuteNonQuery();
            scheduleNumber++;
        }
    }

    private static List<Station> GetStations(int layoutId, string connectionString)
    {
        using var connection = new OdbcConnection(connectionString);
        var command = new OdbcCommand($"SELECT * FROM XplnGetStations WHERE LayoutId = {layoutId};")
        {
            Connection = connection
        };
        command.Connection.Open();
        var reader = command.ExecuteReader();
        var result = new List<Station>();
        Station? station = null;
        var lastStationId = 0;
        while (reader.Read())
        {
            var stationId = reader.GetInt32(reader.GetOrdinal("StationId"));
            if (station is not null)
            {
                if (stationId != lastStationId)
                {
                    result.Add(station);
                    station = new Station() { Id = stationId, Signature = reader.GetString(reader.GetOrdinal("Signature")) };
                }
            }
            else
            {
                station = new Station() { Id = stationId, Signature = reader.GetString(reader.GetOrdinal("Signature")) };

            }
            var track = new StationTrack(reader.GetString(reader.GetOrdinal("TrackNumber")), true, true);
            track.SetId(reader.GetInt32(reader.GetOrdinal("TrackId")));
            station.Add(track);
            lastStationId = stationId;

        }
        if (station != null) result.Add(station);
        return result;
    }
}
