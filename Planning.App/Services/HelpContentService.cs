using System.Globalization;
using Tellurian.Localization;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

/// <summary>
/// Loads and caches the per-tab "About this tab" help, rendered from the localised markdown
/// file <c>Content/Help/{key}.md</c> (resource key <c>Help-{key}</c>). The underlying HTTP
/// markdown provider performs a fresh request per call, so results are cached per
/// (culture, key) — the help is small, static and read many times as popovers open.
/// </summary>
public sealed class HelpContentService([FromKeyedServices("Markdown")] IResourceProvider markdown)
{
    private readonly Dictionary<string, string> _cache = [];

    /// <summary>Returns the help as HTML, or an empty string when no help file exists for the key.</summary>
    public async ValueTask<string> GetHtmlAsync(string helpKey, CultureInfo culture)
    {
        var cacheKey = $"{culture.TwoLetterISOLanguageName}:{helpKey}";
        if (_cache.TryGetValue(cacheKey, out var cached)) return cached;

        var contentKey = $"Help-{helpKey}";
        var content = await markdown.GetTranslationAsync(contentKey, culture);

        // The provider returns the key itself when no file is found; treat that as "no help".
        var html = string.IsNullOrWhiteSpace(content.Text) || content.Text == contentKey
            ? string.Empty
            : Markdig.Markdown.ToHtml(content.Text);

        _cache[cacheKey] = html;
        return html;
    }
}
