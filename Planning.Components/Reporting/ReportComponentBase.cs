using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// Base class of every component a report is printed with, so it prints in the language of the report
/// item it belongs to (see <see cref="Reporting.ReportCulture"/>). Inherited through the folder's
/// <c>_Imports.razor</c>.
/// </summary>
/// <remarks>
/// A derived component calls <see cref="UseReportCulture()"/> as the first statement of its markup —
/// <c>@{ UseReportCulture(); }</c> — because the culture has to be in force while the markup is built, and
/// no earlier hook runs at that moment. A component forgetting it prints in whatever culture the component
/// rendered before it left behind, which in a report of mixed languages is another item's.
/// <para>
/// Used outside a report, where nothing cascades a culture, the call does nothing and the component keeps
/// the interface language.
/// </para>
/// </remarks>
public abstract class ReportComponentBase : ComponentBase
{
    /// <summary>The culture of the report item this component is part of, or <c>null</c> outside a report.</summary>
    [CascadingParameter] public ReportCulture? ReportCulture { get; set; }

    /// <summary>Applies the cascaded <see cref="ReportCulture"/>, if any, for the markup that follows.</summary>
    protected void UseReportCulture() => ReportCulture?.Apply();

    /// <summary>
    /// Applies <paramref name="culture"/> for the markup that follows. For a report page, which is where
    /// the culture is cascaded from rather than to.
    /// </summary>
    protected static void UseReportCulture(ReportCulture? culture) => culture?.Apply();

    /// <inheritdoc/>
    /// <remarks>
    /// Puts the interface culture back once the render is done, so the application around the report —
    /// reports open in the same window — never continues in the report's language.
    /// </remarks>
    protected override void OnAfterRender(bool firstRender) => Reporting.ReportCulture.Restore();
}
