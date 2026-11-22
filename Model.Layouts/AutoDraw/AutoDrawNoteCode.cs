namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>What an auto-draw note is about (ModuleDrawing §8.6). UIs can localise notes by code.</summary>
public enum AutoDrawNoteCode
{
    /// <summary>The standard isn't in the catalogue; nothing is drawn.</summary>
    UnknownStandard,
    /// <summary>The standard is drawn as another standard with an add-on.</summary>
    StandardMapped,
    /// <summary>No exits were given; default exits were assumed.</summary>
    ExitsInferred,
    /// <summary>The exits have no defined side; the first one is drawn as the first exit.</summary>
    ExitDirectionsUndefined,
    /// <summary>Exits with the internal end profile are drawn as joints between sections.</summary>
    InternalExitsBecameJoints,
    /// <summary>An exit's end profile isn't in the catalogue; a default one is used.</summary>
    EndProfileNotFound,
    /// <summary>The standard has no default end profile; nothing is drawn.</summary>
    MissingDefaultEndProfile,
    /// <summary>An end profile has no end-plate outline, so its width is unknown; nothing is drawn.</summary>
    EndProfileWithoutOutline,
    /// <summary>The end-of-line flag doesn't match the number of exits.</summary>
    EndOfLineFlagMismatch,
    /// <summary>There is not enough length, radius or angle to draw the module.</summary>
    NoGeometry,
    /// <summary>An angle was given without a radius; the standard's default radius is used.</summary>
    DefaultRadiusUsed,
    /// <summary>The given width is smaller than the end plate and is ignored.</summary>
    WidthBelowPlateIgnored,
    /// <summary>The module is too short to reach its full width; it is drawn narrower.</summary>
    WideningShortened,
    /// <summary>The given number of through tracks differs from the end profiles; the end profiles are followed.</summary>
    ThroughTracksMismatch,
    /// <summary>A track couldn't be generated and must be drawn by hand.</summary>
    TrackNotGenerated,
    /// <summary>Probably a loop module; the return-loop track must be drawn by hand.</summary>
    LoopModule,
    /// <summary>A turntable or a full-turn angle, drawn as far as possible; check it.</summary>
    CheckAngleOrTurntable,
    /// <summary>A junction: the extra exits are placed provisionally and the track work must be drawn by hand.</summary>
    JunctionNeedsCompletion,
    /// <summary>Sections of a junction module aren't generated; it is drawn as one section.</summary>
    SectionsNotGenerated,
    /// <summary>The parameters don't give a valid drawing; nothing is drawn.</summary>
    InvalidGeometry,
}
