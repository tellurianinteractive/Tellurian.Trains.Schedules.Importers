using System.Globalization;
using Tellurian.Trains.Schedules.Model.Validations;
using Tellurian.Trains.Schedules.Planning.App.Translations;

namespace Tellurian.Trains.Schedules.Planning.App;

/// <summary>
/// GUI presentation helpers for the Model's <see cref="DeletionResult"/>.
/// </summary>
public static class DeletionPresentation
{
    /// <summary>The number of referencing items listed before collapsing the rest into "(and N more)".</summary>
    public const int MaxListed = 2;

    /// <summary>
    /// A short, localised explanation of why an object cannot be deleted. When it is still referenced,
    /// the <paramref name="leadKey"/> message followed by up to <see cref="MaxListed"/> of the
    /// referencing items and "(and N more)" when there are further ones — so the user can find and
    /// remove the references. When a rule of the model forbids the deletion, that rule's sentence.
    /// Returns <c>null</c> when the deletion is allowed, so a bound <c>title</c> attribute is simply
    /// omitted.
    /// </summary>
    /// <param name="deletion">The result of <c>Plan.MayDelete(...)</c>.</param>
    /// <param name="leadKey">Resource key of the lead sentence, e.g. <c>CompanyReferenced</c>.</param>
    /// <param name="translate">The UI translator.</param>
    public static string? BlockedReason(DeletionResult? deletion, string leadKey, Translator translate)
    {
        if (deletion is DeletionResult.NotAllowed notAllowed) return translate(notAllowed.ReasonKey);
        if (deletion is not DeletionResult.Failure failure) return null;
        var references = failure.References;
        var listed = string.Join(", ", references.Take(MaxListed).Select(r => r.ToString()));
        var extra = references.Count - MaxListed;
        if (extra > 0)
            listed = $"{listed} {string.Format(CultureInfo.CurrentCulture, translate("AndMore"), extra)}";
        return $"{translate(leadKey)} {listed}";
    }
}
