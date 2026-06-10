using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Runtime.CompilerServices;
using Tellurian.Trains.Schedules.Importers.Access.Extensions;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

[assembly: InternalsVisibleTo("Tellurian.Trains.Schedules.Importers.Access.Tests")]

namespace Tellurian.Trains.Schedules.Importers.Access;

/// <summary>
/// Repository for importing and persisting railway schedule data from Microsoft Access databases.
/// </summary>
/// <remarks>
/// This class provides functionality to read layouts, timetables, trains, and related railway
/// schedule data from Microsoft Access database files using ODBC connections.
/// </remarks>
/// <param name="databaseFile">The Microsoft Access database file to connect to.</param>
/// <param name="logger">The logger instance for recording import operations and messages.</param>
public class AccessRepository(FileInfo databaseFile, ILogger<AccessRepository> logger) : IImportService
{
    private readonly FileInfo DatabaseFile = databaseFile;
    private readonly ILogger Logger = logger;

    private void LogMessages(IEnumerable<Message> messages)
    {
        foreach (var message in messages)
        {
            if (message.Severity == Severity.None) return;
            if (message.Severity == Severity.Error && Logger.IsEnabled(LogLevel.Error)) Logger.LogError("Error {ErrorMessage}", message.ToString());
            else if (message.Severity == Severity.Warning && Logger.IsEnabled(LogLevel.Warning)) Logger.LogWarning("Warning {WarningMessage}", message.ToString());
            else if (message.Severity == Severity.Information && Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("Information {InformationMessage}", message.ToString());
            else if (message.Severity == Severity.System && Logger.IsEnabled(LogLevel.Critical)) Logger.LogCritical("Critical {CriticalMessage}", message.ToString());
        }
    }

    /// <summary>
    /// Imports a complete schedule from the Access database.
    /// </summary>
    /// <param name="name">The name of the schedule/layout to import.</param>
    /// <returns>An import result containing the schedule if successful, or error messages if the import failed.</returns>
    public Task<ImportResult<Plan>> ImportScheduleAsync(string name)
    {
        var layout = GetLayout(name);
        if (layout.IsFailure)
        {
            var result = new ImportResult<Plan>() { Messages = layout.Messages };
            LogMessages(result.Messages);
            return Task.FromResult(result);
        }
        var timetable = new Timetable(name, layout.Item);
        var importResult = ImportResult<Plan>.SuccessIfNoErrorMessagesOtherwiseFailure(Plan.Create(name, timetable), layout.Messages);
        LogMessages(importResult.Messages);
        return Task.FromResult(importResult);
    }

    private ImportResult<Layout> GetLayout(string name)
    {
        var layout = ReadLayout(name);
        if (layout is not null)
        {
            ReadLayoutStations(layout);
            ReadStationTracks(layout);
            ReadTrackStretches(layout);
            ReadTimetableStretches(layout);
            return ImportResult<Layout>.Success(layout);
        }
        return ImportResult<Layout>.Failure(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrackLayoutDoesNotExist, name)));
    }

    private Layout? ReadLayout(string layoutName)
    {
        using var command = Layouts.CreateSelectCommand(layoutName);
        command.Connection = CreateConnection();
        command.Connection.Open();
        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
        {
            if (reader.Read())
            {
                return new Layout { Name = (reader.GetString(0)) };
            }
        }
        command.Connection.Close();
        return null;
    }

    private ImportResult<Timetable> GetTimetable(string name, Layout layout)
    {
        var timetable = new Timetable(name, layout);
        GetTrains(timetable);

        return new ImportResult<Timetable>() { Items = [timetable], Messages = [] };
    }

    private void ReadLayoutStations(Layout layout)
    {
        using var command = LayoutStations.CreateSelectCommand(layout.Name);
        GetData(command, layout, Stations.RecordHandler);
    }

    private void ReadStationTracks(Layout layout)
    {
        using var command = LayoutStationTracks.CreateSelectCommand(layout.Name);
        GetData(command, layout, StationTracks.RecordHandler);
    }

    private void ReadTrackStretches(Layout layout)
    {
        using var command = TrackStretches.CreateSelectCommand(layout.Name);
        GetData(command, layout, TrackStretches.RecordHandler);
    }

    private void ReadTimetableStretches(Layout layout)
    {
        using var command = TimetableStretches.CreateSelectCommand(layout.Name);
        GetData(command, layout, TimetableStretches.RecordHandler);
    }

    private void GetTrains(Timetable timetable)
    {
        using var command = Trains.CreateSelectCommand(timetable.Name);
        GetData(command, timetable, Trains.RecordHandler, Trains.FinalHandler);
    }

    private void GetData<T>(IDbCommand command, T container, Action<IDataRecord, T> itemHandler)
    {
        GetData(command, container, itemHandler, null);
    }

    private void GetData<T>(IDbCommand command, T container, Action<IDataRecord, T> itemHandler, Action<T>? finalHandler)
    {
        using (var connection = CreateConnection())
        {
            var reader = connection.ExecuteReader(command);
            while (reader.Read())
            {
                itemHandler.Invoke(reader, container);
            }
        }
        finalHandler?.Invoke(container);
    }

    /// <summary>
    /// Saves a layout to the Access database.
    /// </summary>
    /// <param name="layout">The layout to save.</param>
    /// <returns>An import result indicating success or failure with appropriate messages.</returns>
    /// <remarks>
    /// Only new layouts can be saved; updating existing layouts is not supported.
    /// The layout including stations, station tracks, track stretches, and timetable stretches will be persisted.
    /// </remarks>
    public ImportResult<Layout> Save(Layout layout)
    {
        var existing = ReadLayout(layout.Name);
        if (existing is not null)
            return ImportResult<Layout>.Failure(Message.Error(string.Format(CultureInfo.CurrentCulture, "Can only save new track layout, not update existing layout {0}.", layout.Name)));
        using var command = Layouts.CreateInsertCommand(layout);
        _ = ExecuteNonQuery(command);

        var layoutId = (int?)ExecuteScalar(CreateCommand("SELECT Id FROM Layout WHERE [Name] = '" + layout.Name + "'"));
        if (layoutId.HasValue)
        {
            foreach (var station in layout.OperationLocations) Stations.AddOrUpdateStation(layoutId.Value, station, CreateConnection());
            foreach (var stretch in layout.TrackStretches) TrackStretches.AddTrackStretches(layoutId.Value, stretch, CreateConnection());
            foreach (var stretch in layout.TimetableStretches) TimetableStretches.AddTimetableStretches(layoutId.Value, stretch, CreateConnection());
            return ImportResult<Layout>.Success();

        }
        return ImportResult<Layout>.Failure(Message.Error(string.Format(CultureInfo.CurrentCulture, "Layout {0} does not exist.", layout.Name)));
    }

    /// <summary>
    /// Saves a timetable to the Access database.
    /// </summary>
    /// <param name="timetable">The timetable to save.</param>
    /// <returns>An import result indicating success or failure with appropriate messages.</returns>
    /// <remarks>
    /// Only new timetables can be saved; updating existing timetables is not supported.
    /// The timetable's layout will also be saved if it does not already exist.
    /// </remarks>
    public ImportResult<Timetable> Save(Timetable timetable)
    {
        var existing = ReadLayout(timetable.Name);
        if (existing is not null)
            return ImportResult<Timetable>.Failure(Message.Error(string.Format(CultureInfo.CurrentCulture, "Can only save new timetable, not update existing timetable {0}.", timetable.Name)));
        Save(timetable.Layout);
        using var command = CreateCommand("SELECT Id FROM Layout WHERE [Name] = @Name", "@Name", timetable.Name);
        var layoutId = (int?)ExecuteScalar(CreateConnection(), command);
        if (layoutId.HasValue)
        {
            foreach (var train in timetable.Trains) Trains.Add(layoutId.Value, train, this);
            return ImportResult<Timetable>.Success();
        }
        return ImportResult<Timetable>.Failure(Message.Error(string.Format(CultureInfo.CurrentCulture, "Layout {0} does not exist.", timetable.Layout.Name)));
    }

    internal int Delete(string layoutName)
    {
        using var command = Layouts.CreateDeleteCommand(layoutName);
        return ExecuteNonQuery(CreateConnection(), command);
    }


    internal object? ExecuteScalar(IDbCommand command)
    {
        return ExecuteScalar(CreateConnection(), command);
    }

    internal static object? ExecuteScalar(IDbConnection connection, string sql)
    {
        using var command = CreateCommand(sql);
        return ExecuteScalar(connection, command);
    }

    internal static object? ExecuteScalar(IDbConnection connection, IDbCommand command)
    {
        command.Connection = connection;
        try
        {
            command.Connection.Open();
            return command.ExecuteScalar();
        }
        finally
        {
            connection.Close();
        }
    }

    internal int ExecuteNonQuery(IDbCommand command)
    {
        return ExecuteNonQuery(CreateConnection(), command);
    }

    internal static int ExecuteNonQuery(IDbConnection connection, IDbCommand command)
    {
        command.Connection = connection;
        try
        {
            command.Connection.Open();
            return command.ExecuteNonQuery();
        }
        finally
        {
            connection.Close();
        }
    }

    internal static IDbCommand CreateCommand(string sql)
    {
        return new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = sql,
        };
    }

    internal static IDbCommand CreateCommand(string sql, string parameterName, string parameterValue)
    {
        var result = CreateCommand(sql);
        result.Parameters.Add(new OdbcParameter(parameterName, parameterValue));
        return result;
    }

    internal IDbConnection CreateConnection() => OdbcConnectionExtensions.CreateMicrosoftAccessDbConnection(DatabaseFile.FullName);
}
