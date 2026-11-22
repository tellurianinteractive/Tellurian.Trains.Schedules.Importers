namespace Tellurian.Trains.Schedules.Model.Layouts;

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
            new() { Id = 1, ShortName = "N", Denominator = 160 },
            new() { Id = 2, ShortName = "TT", Denominator = 120 },
            new() { Id = 3, ShortName = "H0", Denominator = 87 },
            new() { Id = 4, ShortName = "00", Denominator = 76 },
            new() { Id = 5, ShortName = "0", Denominator = 45, Remark = "NEM" },
            new() { Id = 6, ShortName = "I", Denominator = 32 },
            new() { Id = 101, ShortName = "0", Denominator = 48, Remark = "NMRA" },
        ];

        /// <summary>
        /// Finds the <see cref="Scale"/> with the given <see cref="Scale.Id"/>, or <c>null</c> when none matches.
        /// </summary>
        public static Scale? ById(int id) => Scale.Scales.FirstOrDefault(s => s.Id == id);
    }
}
