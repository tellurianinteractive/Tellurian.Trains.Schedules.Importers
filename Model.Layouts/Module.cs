using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A module: what it is called and who owns it, which is the same wherever it is set up. A module can be an
/// operation location; a module that is not is plain line.
/// </summary>
public class Module : LayoutPart
{
    /// <summary>The name the owner has given the module, e.g. "Tellby" or "Curve 30°".</summary>
    public required string Name { get; set; }

    /// <summary>The letters the owner marks modules with, e.g. "SF"; null when there are none.</summary>
    public string? Prefix { get; set; }

    /// <summary>The owner's number of the module, e.g. 12; null when it has none.</summary>
    public int? Number { get; set; }

    /// <summary>
    /// The <see cref="Prefix"/> and <see cref="Number"/> are a FREMO module name: an owning person is a FREMO member
    /// with a FREMO prefix. Only then may the designation be presented as a FREMO name.
    /// </summary>
    public bool HasFremoDesignation { get; set; }

    /// <summary>Tells apart the ways one module can be set up, e.g. "Short"; null when there is only one.</summary>
    public string? ConfigurationLabel { get; set; }

    /// <summary>The box the module is transported in; null when it comes on its own.</summary>
    public string? PackageLabel { get; set; }

    /// <summary>
    /// The names of all owners, separated by commas, e.g. "Stefan Fjällemark, Module Club". For reading only: who
    /// owns a module is kept by the Module Registry, which sets or overwrites this when a drawing is uploaded.
    /// </summary>
    public string? ModuleOwners { get; set; }

    /// <summary>The standard the module is built to, which also tells its scale; null when not stated.</summary>
    public ModuleStandard? Standard { get; set; }

    /// <summary>The operation location that the module is; null for a module of plain line.</summary>
    public OperationLocation? OperationLocation { get; set; }

    /// <summary>
    /// The tracks of the module: the tracks of plain line, or those of its operation location. In a layout, a module
    /// that is an operation location has the very same tracks as <see cref="OperationLocation.Tracks"/>.
    /// </summary>
    public IList<StationTrack> Tracks { get; set; } = [];

    /// <summary>
    /// A copy with copies of the module's operation location and tracks. When the tracks are those of the location,
    /// the copy's are those of the copied location too. The standard is referred to, not copied.
    /// </summary>
    public override Module Copy()
    {
        var copy = (Module)base.Copy();
        copy.OperationLocation = OperationLocation?.Copy();
        copy.Tracks = OperationLocation is not null && ReferenceEquals(Tracks, OperationLocation.Tracks)
            ? copy.OperationLocation!.Tracks
            : [.. Tracks.Select(track => track.Copy())];
        return copy;
    }

    /// <inheritdoc/>
    public override string ToString() => this.DisplayName;
}

/// <summary>What a <see cref="Module"/> is called, worked out from what is given: prefix, number and name.</summary>
public static class ModuleExtensions
{
    extension(Module module)
    {
        /// <summary>
        /// The prefix and the number, as far as they are given: the prefix, a space and the number with four digits,
        /// "SF 0007"; without a prefix the number alone, as it is, "12". Null when the module has no number.
        /// </summary>
        public string? Designation =>
            module.Number is not int number ? null :
            HasText(module.Prefix) ? string.Create(CultureInfo.InvariantCulture, $"{module.Prefix!.Trim()} {number:0000}") :
            number.ToString(CultureInfo.InvariantCulture);

        /// <summary>The scale of the module: that of its standard. Null when not stated.</summary>
        public Scale? Scale => module.Standard?.Scale;

        /// <summary>
        /// What the module is called, without its standard and scale: what is written on it and where its standard
        /// goes without saying. With a prefix and a number: the designation, followed by the name of its operation
        /// location when it is one, "SF 0007 Tellby". With a number and no prefix: its name followed by the number,
        /// "Curve 12". Without a number: its name, followed by the configuration label in lower case when it has one,
        /// "Tellby short". Parts that are not given are left out; empty when nothing is.
        /// </summary>
        public string Title =>
            module.Designation is string designation && HasText(module.Prefix) ? Join(designation, module.OperationLocation?.Name) :
            module.Designation is string number ? Join(module.Name, number) :
            Join(module.Name, module.ConfigurationLabel?.Trim().ToLowerInvariant());

        /// <summary>
        /// What the module is called in lists: its standard and scale, when stated, followed by its
        /// <c>Title</c>, without a word said twice (<see cref="Wording.JoinWithoutRepeatedWords"/>).
        /// Standard "FREMO/H0-RE" with scale "H0" gives "FREMO/H0-RE SF 0007 Tellby".
        /// </summary>
        public string DisplayName => Wording.JoinWithoutRepeatedWords(module.Standard?.ShortName, module.Scale?.ShortName, module.Title);
    }

    private static string Join(params IEnumerable<string?> parts) => string.Join(' ', parts.Where(HasText).Select(p => p!.Trim()));

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(value);
}
