using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>
/// The straight edge at an exit, as wide as its end profile (ModuleDrawing §4.2). The corners are
/// named as seen standing outside the module looking at the plate, matching the end view.
/// </summary>
public sealed record EndPlate(ModuleExit Exit, double Width, Point LeftCorner, Point RightCorner);
