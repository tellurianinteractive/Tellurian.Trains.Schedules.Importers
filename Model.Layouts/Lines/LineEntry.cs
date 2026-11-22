using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// One module in a <see cref="Line"/>, with what the planner can change about its placement (HP-16). Not turned, the
/// module's left exit joins the previous module and its right exit the next; turned 180°, the other way round. Nothing
/// inside the module drawing is referred to, so a module can be redrawn or replaced without the layout breaking. Its
/// pose is worked out, not stored. An end-of-line module has one exit, which joins its only neighbour.
/// </summary>
public sealed class LineEntry : LayoutPart
{
    /// <summary>The <see cref="LayoutModule.Id"/> of the module; a module is in at most one line, once (HP-7).</summary>
    public int ModuleId { get; set; }

    /// <summary>The module is turned 180°: its right exit joins the previous module (HP-4).</summary>
    public bool IsTurned { get; set; }

    /// <summary>
    /// For a module with three or more exits: the exit the line continues from. Null means the first exit on the side
    /// facing the next module, which is all a module with two exits needs.
    /// </summary>
    public ExitPlace? ContinuesFrom { get; set; }

    /// <summary>Raises (positive) or lowers the whole module, mm; 0 is the standard's height (HP-13).</summary>
    public double LegAdjustment { get; set; }

    /// <summary>A note for the people setting up.</summary>
    public string? Note { get; set; }

    /// <summary>The exit that joins the previous module in the line.</summary>
    [JsonIgnore]
    public ExitPlace Entry => IsTurned ? ExitPlace.Right : ExitPlace.Left;

    /// <summary>The exit that the line continues from, to the next module.</summary>
    [JsonIgnore]
    public ExitPlace Exit => ContinuesFrom ?? (IsTurned ? ExitPlace.Left : ExitPlace.Right);

    /// <summary>A copy turned around, so that the module's other end joins the previous module (HP-4).</summary>
    public LineEntry Flipped() => this.With(e => e.IsTurned = !IsTurned);

    /// <inheritdoc/>
    public override LineEntry Copy() => (LineEntry)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => IsTurned ? $"{ModuleId} turned" : ModuleId.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
