using System.Runtime.CompilerServices;
using SQLitePCL;

namespace Tellurian.Trains.Schedules.Model.Databases.Tests;

/// <summary>
/// Registers the SQLitePCLRaw e_sqlite3 provider once for the whole test assembly.
/// The bundle_e_sqlite3 meta-package normally does this via Batteries_V2.Init(), but we
/// avoid it (its native build is deprecated/vulnerable) and supply the native from
/// SourceGear.sqlite3, so we register the provider ourselves.
/// </summary>
internal static class SqliteProviderInitializer
{
    [ModuleInitializer]
    internal static void Init() => raw.SetProvider(new SQLite3Provider_e_sqlite3());
}
