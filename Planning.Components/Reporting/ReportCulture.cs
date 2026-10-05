using System.Globalization;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// The culture a part of a report is printed in, cascaded from the report page to every report component
/// beneath it. A page cascades the layout's default culture around the whole report, and a report printing
/// each item in its local language cascades that item's culture around the item.
/// </summary>
/// <remarks>
/// Printing is done in the <em>ambient</em> culture — <see cref="CultureInfo.CurrentUICulture"/> and
/// <see cref="CultureInfo.CurrentCulture"/> — because that is what every translation and every model text
/// (notes, day names, formatted numbers) reads. It is applied rather than passed on: each report
/// component applies its cascaded culture when it renders, and the renderer restores the interface
/// culture once the report has rendered (see <see cref="ReportComponentBase"/>).
/// <para>
/// Blazor renders a parent's child components after it and interleaved with their siblings, so setting
/// the culture once around an item does not carry to its components: each must apply it again. Anything
/// computed before rendering that reads the culture — pagination measuring note texts, for instance —
/// has to be computed inside a <see cref="Use"/> scope for the same reason.
/// </para>
/// </remarks>
/// <param name="Culture">The culture to print in.</param>
public sealed record ReportCulture(CultureInfo Culture)
{
    /// <summary>Makes <see cref="Culture"/> the ambient culture, until another is applied or it is restored.</summary>
    public void Apply()
    {
        CultureInfo.CurrentUICulture = Culture;
        CultureInfo.CurrentCulture = Culture;
    }

    /// <summary>
    /// Makes <paramref name="culture"/> the ambient culture until the returned scope is disposed, which
    /// puts back the culture that was ambient before. For work done outside rendering, such as building
    /// the pages of an item.
    /// </summary>
    public static IDisposable Use(CultureInfo culture) => new Scope(culture);

    /// <summary>Puts back the interface culture the application was started with.</summary>
    public static void Restore()
    {
        if (CultureInfo.DefaultThreadCurrentUICulture is { } ui) CultureInfo.CurrentUICulture = ui;
        if (CultureInfo.DefaultThreadCurrentCulture is { } formatting) CultureInfo.CurrentCulture = formatting;
    }

    private sealed class Scope : IDisposable
    {
        private readonly CultureInfo _previousUi = CultureInfo.CurrentUICulture;
        private readonly CultureInfo _previous = CultureInfo.CurrentCulture;

        public Scope(CultureInfo culture) => new ReportCulture(culture).Apply();

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = _previousUi;
            CultureInfo.CurrentCulture = _previous;
        }
    }
}
