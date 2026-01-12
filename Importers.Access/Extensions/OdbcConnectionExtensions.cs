using System.Data;
using System.Data.Odbc;
using System.Globalization;


namespace Tellurian.Trains.Schedules.Importers.Access.Extensions;

/// <summary>
/// Provides extension methods for working with ODBC connections to Microsoft Access databases.
/// </summary>
public static class OdbcConnectionExtensions
{
    const string driver = "{Microsoft Access Driver (*.mdb, *.accdb)}";

    /// <summary>
    /// Creates an ODBC connection string for a Microsoft Access database file.
    /// </summary>
    /// <param name="databaseFilePath">The full path to the Microsoft Access database file (.mdb or .accdb).</param>
    /// <returns>A properly formatted ODBC connection string for the specified database file.</returns>
    public static string ConnectionString(this string databaseFilePath) =>
        string.Format(CultureInfo.InvariantCulture, "Driver={0};DBQ={1}", driver, databaseFilePath);

    /// <summary>
    /// Creates a new ODBC connection to a Microsoft Access database.
    /// </summary>
    /// <param name="databaseFilePath">The full path to the Microsoft Access database file (.mdb or .accdb).</param>
    /// <returns>A new ODBC connection instance configured for the specified database file.</returns>
    public static IDbConnection CreateMicrosoftAccessDbConnection(string databaseFilePath) =>
        new OdbcConnection(ConnectionString(databaseFilePath));

    /// <summary>
    /// Executes a command and returns a data reader.
    /// </summary>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="command">The database command to execute.</param>
    /// <returns>A data reader for reading the command results. The connection will be closed when the reader is closed.</returns>
    public static IDataReader ExecuteReader(this IDbConnection connection, IDbCommand command)
    {
        command.Connection = connection;
        command.Connection.Open();
        return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of rows affected.
    /// </summary>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="command">The database command to execute.</param>
    /// <returns>The number of rows affected by the command.</returns>
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

    /// <summary>
    /// Executes a SQL query and returns the first column of the first row as a scalar value.
    /// </summary>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The first column of the first row, or null if no results.</returns>
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
