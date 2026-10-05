using System.Globalization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Decides the language and culture a report, or one item of a report, is printed in.
/// </summary>
/// <remarks>
/// Reports are printed in the layout's default language — the first language of its default country —
/// never in the user-interface language, because the paper is read by the participants at the meeting,
/// not by the planner. When <see cref="GeneralSettings.UseObjectLanguageInReports"/> is set, each item
/// is instead printed in its own local language, found by the rules of the item's own method here, and
/// falls back to the default language when no local language can be found.
/// <para>
/// A language counts as found only when the application can print in it (the <c>isAvailable</c>
/// predicate); an unsupported language is passed over as if it were not set, so a Dutch company falls
/// back to the next rule rather than to the resources' neutral language.
/// </para>
/// <para>
/// The culture is composed as <c>{language}-{CountryCode}</c>, so dates and numbers follow the country
/// even where it shares its language with another (<c>de-CH</c> rather than <c>de-DE</c>).
/// </para>
/// </remarks>
public static class ReportCultureExtensions
{
    private const string FallbackLanguage = "en";

    extension(Country country)
    {
        /// <summary>
        /// The country's first language — its default report language — as a two-letter ISO code, or an
        /// empty string when the country lists none.
        /// </summary>
        public string PrimaryLanguage =>
            country.Languages
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// The culture of <c>PrimaryLanguage</c> in this country, for example <c>de-CH</c>, or the
        /// language's neutral culture when the combination is unknown to the platform.
        /// </summary>
        public CultureInfo PrimaryCulture => CultureOf(country.PrimaryLanguage, country.CountryCode);
    }

    extension(Layout layout)
    {
        /// <summary>
        /// The layout's default language: the first language of its default country, or English when the
        /// default country cannot be resolved. Every report is printed in it unless local languages are
        /// used, and then still every item that has no local language of its own.
        /// </summary>
        public string DefaultLanguage =>
            layout.DefaultCountry?.PrimaryLanguage is { Length: > 0 } language ? language : FallbackLanguage;

        /// <summary>
        /// The culture reports are printed in by default: <c>DefaultLanguage</c> in the layout's
        /// default country.
        /// </summary>
        public CultureInfo DefaultReportCulture =>
            CultureOf(layout.DefaultLanguage, layout.DefaultCountry?.CountryCode);

        /// <summary>
        /// The culture to print an item in, given the company the item belongs to and the companies
        /// operating the trains it covers: the company's language; failing that, the language of the
        /// operators when they all share one; failing that, <c>DefaultReportCulture</c>. Always the
        /// default culture when <see cref="GeneralSettings.UseObjectLanguageInReports"/> is off.
        /// </summary>
        /// <remarks>
        /// A train with no operator, or an operator with no country, takes no part in the vote: it neither
        /// supplies a language nor breaks the agreement of the others.
        /// </remarks>
        /// <param name="company">The company the item belongs to, or <c>null</c> when it has none.</param>
        /// <param name="operators">The operating companies of the trains the item covers.</param>
        /// <param name="isAvailable">Whether reports can be printed in a two-letter language.</param>
        public CultureInfo ReportCultureOf(Company? company, IEnumerable<Company?> operators, Func<string, bool> isAvailable)
        {
            if (!layout.Settings.General.UseObjectLanguageInReports) return layout.DefaultReportCulture;
            if (layout.AvailableCountryOf(company, isAvailable) is { } own) return own.PrimaryCulture;

            var countries = operators
                .Select(o => layout.CountryById(o?.CountryId))
                .OfType<Country>()
                .Where(c => c.PrimaryLanguage.HasValue)
                .ToList();
            return countries is [var first, ..]
                && countries.All(c => c.PrimaryLanguage.Equals(first.PrimaryLanguage, StringComparison.OrdinalIgnoreCase))
                && isAvailable(first.PrimaryLanguage)
                    ? first.PrimaryCulture
                    : layout.DefaultReportCulture;
        }

        private Country? AvailableCountryOf(Company? company, Func<string, bool> isAvailable) =>
            layout.CountryById(company?.CountryId) is { } country && isAvailable(country.PrimaryLanguage)
                ? country
                : null;
    }

    extension(DriverDuty duty)
    {
        /// <summary>
        /// The culture to print the duty's booklet in: the language of the company working the duty,
        /// else of the operators of its trains when they agree, else the layout's default. See
        /// <c>Layout.ReportCultureOf</c>.
        /// </summary>
        /// <param name="layout">The layout the duty is worked on.</param>
        /// <param name="isAvailable">Whether reports can be printed in a two-letter language.</param>
        public CultureInfo ReportCulture(Layout layout, Func<string, bool> isAvailable) =>
            layout.ReportCultureOf(duty.Company, duty.Parts.Select(p => p.Train.EffectiveCompany), isAvailable);
    }

    extension(OperationLocation location)
    {
        /// <summary>
        /// The culture to print the location's sheets in: the language of the country it lies in, else
        /// the layout's default. Always the default when
        /// <see cref="GeneralSettings.UseObjectLanguageInReports"/> is off.
        /// </summary>
        /// <param name="layout">The layout the location belongs to.</param>
        /// <param name="isAvailable">Whether reports can be printed in a two-letter language.</param>
        public CultureInfo ReportCulture(Layout layout, Func<string, bool> isAvailable) =>
            layout.Settings.General.UseObjectLanguageInReports
                && layout.CountryById(location.CountryId) is { } country
                && isAvailable(country.PrimaryLanguage)
                    ? country.PrimaryCulture
                    : layout.DefaultReportCulture;
    }

    private static CultureInfo CultureOf(string language, string? countryCode)
    {
        if (language.IsEmpty) language = FallbackLanguage;
        try
        {
            return countryCode.HasValue
                ? CultureInfo.GetCultureInfo($"{language}-{countryCode}")
                : CultureInfo.GetCultureInfo(language);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.GetCultureInfo(language);
        }
    }
}
