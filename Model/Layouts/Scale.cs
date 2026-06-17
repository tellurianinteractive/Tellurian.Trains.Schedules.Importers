namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Represents a model railway scale.
/// </summary>
/// <param name="Id">A stable unique identifier, used as the reference key from a layout. Once
/// assigned it must never be reused for a different scale (only append new ones).</param>
/// <param name="Name">The common scale name, for example <c>H0</c> or <c>N</c>.</param>
/// <param name="Denominator">The scale ratio denominator (1:<paramref name="Denominator"/>).</param>
/// <param name="Gauge">The standard-gauge track width in millimetres.</param>
/// <param name="Remark">An optional discriminator shown in the user interface, for example the
/// standard (<c>NEM</c> or <c>NMRA</c>) that distinguishes scales sharing a name but differing in ratio.</param>
/// <remarks>
/// The available scales are provided by <c>ScalesService</c> in <c>Importers.Services</c>.
/// </remarks>
public record Scale(int Id, string Name, int Denominator, double Gauge, string Remark = "");

/// <summary>
/// </summary>
public static class ScaleExtensions
{
    extension(Scale scale)
    {
        /// <summary>
        /// Gets the available <see cref="Scale">scales</see>, with the standard-gauge track width in
        /// millimetres. Ids 1-6 match the Module Registry; the 0 scale's alternative NMRA ratio (1:48)
        /// uses a 101+ id outside that range. Scales sharing a name but differing in ratio are
        /// distinguished by <see cref="Scale.Remark"/> (<c>NEM</c> = 1:45, <c>NMRA</c> = 1:48), shown in
        /// the drop-down. The <see cref="Scale.Id"/> values are a stable contract: never reuse one, only append.
        /// </summary>
        public static IEnumerable<Scale> Scales =>
        [
            new(1, "N", 160, 9.0),
            new(2, "TT", 120, 12.0),
            new(3, "H0", 87, 16.5),
            new(4, "00", 76, 16.5),
            new(5, "0", 45, 32.0, "NEM"),
            new(6, "I", 32, 45.0),
            new(101, "0", 48, 32.0, "NMRA"),
        ];

        /// <summary>
        /// Finds the <see cref="Scale"/> with the given <see cref="Scale.Id"/>, or <c>null</c> when none matches.
        /// </summary>
        public static Scale? ById(int id) => Scale.Scales.FirstOrDefault(s => s.Id == id);
    }
}
