namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The suite's list of scales, which the planner, Module Draw and the Module Registry share (decision 11 of
/// <c>CrossAppReview.md</c>): each with an <see cref="Scale.Id"/> that is a stable contract.
/// </summary>
public static class ScaleExtensions
{
    extension(Scale scale)
    {
        /// <summary>
        /// The known <see cref="Scale">scales</see>. Ids 1-6 match the Module Registry; the 0 scale's alternative NMRA
        /// ratio (1:48) uses a 101+ id outside that range. Scales sharing a name but differing in ratio are told apart
        /// by <see cref="Scale.Remark"/> (<c>NEM</c> = 1:45, <c>NMRA</c> = 1:48). The <see cref="Scale.Id"/> values are a
        /// stable contract: never reuse one, only append.
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

        /// <summary>The known <see cref="Scale"/> with the given <see cref="Scale.Id"/>, or <c>null</c> when none matches.</summary>
        public static Scale? ById(int id) => Scale.Scales.FirstOrDefault(s => s.Id == id);

        /// <summary>
        /// The known scale that <paramref name="named"/> is, e.g. a scale in a catalogue or a module, which is written by
        /// short name and often without an id: by its id when it has one; else by short name and ratio; else by short
        /// name when only one known scale has it. Null when <paramref name="named"/> is null or no known scale matches,
        /// e.g. for "0" without a ratio, which is both 1:45 and 1:48.
        /// </summary>
        public static Scale? Known(Scale? named)
        {
            if (named is null) return null;
            if (named.Id is int id) return Scale.ById(id);
            var byName = Scale.Scales.Where(s => string.Equals(s.ShortName, named.ShortName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            if (named.Denominator is decimal denominator) return byName.FirstOrDefault(s => s.Denominator == denominator);
            return byName is [var only] ? only : null;
        }
    }
}
