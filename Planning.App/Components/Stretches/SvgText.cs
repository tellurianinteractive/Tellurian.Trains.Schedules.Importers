using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Tellurian.Trains.Schedules.Planning.App.Components.Stretches;

/// <summary>
/// Renders an SVG <c>&lt;text&gt;</c> element. A dedicated component is needed because Razor treats a bare
/// <c>&lt;text&gt;</c> tag as its own control keyword, so the element cannot be written directly in markup.
/// Used inside an <c>&lt;svg&gt;</c>, so the browser resolves it to the SVG namespace via its DOM parent.
/// </summary>
public sealed class SvgText : ComponentBase
{
    [Parameter] public double X { get; set; }
    [Parameter] public double Y { get; set; }
    [Parameter] public string? Anchor { get; set; }
    [Parameter] public string Fill { get; set; } = "#000";
    [Parameter] public double FontSize { get; set; } = 12;
    [Parameter] public string? FontWeight { get; set; }
    [Parameter] public string? Text { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "text");
        builder.AddAttribute(1, "x", Format(X));
        builder.AddAttribute(2, "y", Format(Y));
        if (Anchor is not null) builder.AddAttribute(3, "text-anchor", Anchor);
        builder.AddAttribute(4, "fill", Fill);
        builder.AddAttribute(5, "font-size", Format(FontSize));
        builder.AddAttribute(6, "font-family", "sans-serif");
        if (FontWeight is not null) builder.AddAttribute(7, "font-weight", FontWeight);
        builder.AddContent(8, Text);
        builder.CloseElement();
    }

    private static string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
