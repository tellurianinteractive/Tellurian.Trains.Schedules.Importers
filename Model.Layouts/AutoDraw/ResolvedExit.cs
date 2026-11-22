using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>An exit with its end profile found in the catalogue (or inferred).</summary>
internal sealed record ResolvedExit(string Label, ExitSide Direction, EndProfile Profile)
{
    public double PlateWidth { get; } = EndProfileAnalysis.PlateWidth(Profile);
}
