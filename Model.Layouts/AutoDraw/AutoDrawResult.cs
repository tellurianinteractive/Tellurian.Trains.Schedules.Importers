using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>The result of <see cref="ModuleAutoDraw.Create"/>.</summary>
public sealed record AutoDrawResult
{
    /// <summary>How far the module could be drawn.</summary>
    public required AutoDrawStatus Status { get; init; }

    /// <summary>The drawn module; null when <see cref="Status"/> is <see cref="AutoDrawStatus.NeedsManualDrawing"/>.</summary>
    public Module? Module { get; init; }

    /// <summary>Every inferred value and every problem.</summary>
    public required IReadOnlyList<AutoDrawNote> Notes { get; init; }

    /// <summary>True when a module was drawn.</summary>
    public bool IsDrawn => Module is not null;
}
