using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>Extensions for applying the selected UI culture from a built host's services.</summary>
public static class UiCultureExtensions
{
    extension(IServiceProvider services)
    {
        /// <summary>
        /// Reads the user's stored UI culture from browser localStorage (see
        /// <see cref="UiCulture"/>) and applies it to the current thread. Call this on the built
        /// host's <see cref="IServiceProvider"/> before <c>RunAsync</c> so the culture is in
        /// effect for the first render. It cannot be an <c>IServiceCollection</c> extension because
        /// it needs JS interop, which is only available once the host has been built.
        /// </summary>
        public async Task ApplyStoredUiCultureAsync()
        {
            var js = services.GetRequiredService<IJSRuntime>();
            var stored = await js.InvokeAsync<string?>("localStorage.getItem", UiCulture.StorageKey);
            var culture = UiCulture.Resolve(stored).CultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
