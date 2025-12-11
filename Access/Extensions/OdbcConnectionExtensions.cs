using System.Data;
using System.Data.Odbc;
using System.Globalization;


namespace Tellurian.Trains.Timetables.Importers.Access.Extensions;

public static class OdbcConnectionExtensions
{
    const string driver = "{Microsoft Access Driver (*.mdb, *.accdb)}";

    public static string ConnectionString(this string databaseFilePath) =>
        string.Format(CultureInfo.InvariantCulture, "Driver={0};DBQ={1}", driver, databaseFilePath);

    public static IDbConnection CreateMicrosoftAccessDbConnection(string databaseFilePath) => 
        new OdbcConnection(ConnectionString(databaseFilePath));

    public static IDataReader ExecuteReader(this IDbConnection connection, IDbCommand command)
    {
        command.Connection = connection;
        command.Connection.Open();
        return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    public static int ExecuteNonQuery(this IDbConnection connection, IDbCommand command)
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

    public static object? ExecuteScalar(this IDbConnection connection, string sql)
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

    private static OdbcCommand CreateCommand(string sql)
    {
        return new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = sql,
        };
    }
}
