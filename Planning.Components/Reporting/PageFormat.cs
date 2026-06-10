using Microsoft.AspNetCore.Components;
using Tellurian.Trains.Schedules.Planning.App.Translations;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// Base class for printable page wrappers (e.g. <c>A4LPage</c> landscape, <c>A4PPage</c> portrait).
/// Holds the parameters common to every page format; a derived component supplies only the
/// orientation-specific markup (its CSS class).
/// </summary>
public abstract class PageFormat : ComponentBase
{
    /// <summary>Translation lookup, shared by all page formats (e.g. for the page footer).</summary>
    [Inject] protected Translator Translator { get; set; } = default!;

    /// <summary>The page body.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Page margin in millimetres, applied as a CSS margin inside the page. The printed
    /// <c>@page</c> margin is 0, so this parameter gives each report full control of its margin.
    /// </summary>
    [Parameter] public int Margin { get; set; }

    /// <summary>The page number shown in the footer. A footer is rendered only when this is &gt; 0.</summary>
    [Parameter] public int PageNumber { get; set; }

    /// <summary>Optional text shown alongside the page number in the footer.</summary>
    [Parameter] public string? Footnote { get; set; }

    /// <summary>The <see cref="Margin"/> as a CSS length, e.g. "10mm".</summary>
    protected string MarginStyle => $"{Margin}mm";
}
