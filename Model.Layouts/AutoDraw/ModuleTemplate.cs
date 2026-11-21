using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>
/// The quick-create templates (MD-3): a straight, a curved and a straight + curve module with two exits,
/// "A" and "B". Each exit can have its own end profile, also of another standard of the same scale, e.g. FREMO at
/// one end and MMM at the other. Templates are drawn with auto-draw (MD-20).
/// </summary>
public sealed record ModuleTemplate
{
    /// <summary>The label of the first exit.</summary>
    public const string FirstExit = "A";
    /// <summary>The label of the second exit.</summary>
    public const string SecondExit = "B";

    /// <summary>Which template: straight, curve, or straight + curve.</summary>
    public required ModuleTemplateKind Kind { get; init; }

    /// <summary>The module's name; may be empty when it has a FREMO number (<see cref="Modules.ModuleValidation"/>).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The owner's number of the module, 1–9999.</summary>
    public int? Number { get; init; }

    /// <summary>The letters the owner marks modules with, e.g. <c>SFjä</c>; null when not known.</summary>
    public string? Prefix { get; init; }

    /// <summary>The names of the owners in one text, separated by commas; null when not known.</summary>
    public string? ModuleOwners { get; init; }

    /// <summary>The standard the module follows.</summary>
    public required ModuleStandard Standard { get; init; }

    /// <summary>The end profile of exit A, and of exit B unless <see cref="SecondEndProfile"/> is given.</summary>
    public required EndProfile EndProfile { get; init; }

    /// <summary>The end profile of exit B; null uses <see cref="EndProfile"/>.</summary>
    public EndProfile? SecondEndProfile { get; init; }

    /// <summary>The module turns an asymmetric end profile over (<i>Endewender</i>).</summary>
    public bool IsEndProfileSwitcher { get; init; }

    /// <summary>The length of a straight module, or of the straight part of a straight + curve module, mm.</summary>
    public double Straight { get; init; }

    /// <summary>The radius of the curve, mm.</summary>
    public double Radius { get; init; }

    /// <summary>The angle of the curve, degrees.</summary>
    public double Angle { get; init; }

    /// <summary>The module's width; null uses the end plate's width.</summary>
    public double? Width { get; init; }

    /// <summary>Which way the curve turns.</summary>
    public CurveDirection CurveDirection { get; init; }
    /// <summary>Where the straight part of a straight + curve module is.</summary>
    public StraightPosition StraightPosition { get; init; }

    /// <summary>The auto-draw parameters for this template.</summary>
    public AutoDrawParameters ToParameters()
    {
        var second = SecondEndProfile ?? EndProfile;
        var exit = new AutoDrawExit { Label = FirstExit, Direction = ExitSide.Left, EndProfile = EndProfile.Designation, EndProfileId = EndProfile.Id };
        var hasStraight = Kind is ModuleTemplateKind.Straight or ModuleTemplateKind.StraightAndCurve;
        var hasCurve = Kind is ModuleTemplateKind.Curve or ModuleTemplateKind.StraightAndCurve;
        return new()
        {
            Name = Name,
            Number = Number,
            Prefix = string.IsNullOrWhiteSpace(Prefix) ? null : Prefix.Trim(),
            ModuleOwners = string.IsNullOrWhiteSpace(ModuleOwners) ? null : ModuleOwners.Trim(),
            Standard = Standard.ShortName,
            Straight = hasStraight ? Straight : null,
            Radius = hasCurve ? Radius : null,
            Angle = hasCurve ? Angle : null,
            Width = Width,
            NumberOfThroughTracks = Math.Max(EndProfile.Tracks.Count, second.Tracks.Count),
            Exits = [exit, exit with { Label = SecondExit, Direction = ExitSide.Right, EndProfile = second.Designation, EndProfileId = second.Id }],
            IsEndProfileSwitcher = IsEndProfileSwitcher,
            CurveDirection = CurveDirection,
            StraightPosition = StraightPosition,
        };
    }

    /// <summary>Draws the module from this template with <paramref name="catalogue"/>.</summary>
    public AutoDrawResult Create(DrawingCatalogue catalogue) => ModuleAutoDraw.Create(ToParameters(), catalogue);
}
