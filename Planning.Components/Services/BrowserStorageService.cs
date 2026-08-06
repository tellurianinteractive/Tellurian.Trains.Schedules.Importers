using System.Text.Json;
using Microsoft.JSInterop;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// Reads and writes strings and JSON-serialisable objects from/to a persistent browser
/// key/value store backed by IndexedDB (via <c>_content/Tellurian.Trains.Schedules.Planning.Components/js/storage.js</c>). IndexedDB is used
/// instead of localStorage because a serialised schedule can exceed the ~5 MB localStorage
/// quota. Registered as a singleton; the JS module is imported once on first use. All
/// operations are best-effort: a storage failure (e.g. private-browsing mode, exceeded quota)
/// is treated as "absent" / a skipped write rather than thrown, so it never blocks the app.
/// </summary>
public sealed class BrowserStorageService(IJSRuntime js) : IAsyncDisposable
{
    private const string ModulePath = "./_content/Tellurian.Trains.Schedules.Planning.Components/js/storage.js";
    private Task<IJSObjectReference>? _moduleTask;

    private Task<IJSObjectReference> ModuleAsync() =>
        _moduleTask ??= js.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();

    /// <summary>Reads a raw string value, or null when the key is absent or storage is unavailable.</summary>
    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var module = await ModuleAsync();
            return await module.InvokeAsync<string?>("get", key);
        }
        catch (JSException)
        {
            return null;
        }
    }

    /// <summary>Writes a raw string value. Best-effort: a storage failure is swallowed.</summary>
    public async Task SetStringAsync(string key, string value)
    {
        try
        {
            var module = await ModuleAsync();
            await module.InvokeVoidAsync("set", key, value);
        }
        catch (JSException)
        {
            // Quota or storage failure — persistence is best-effort, so ignore.
        }
    }

    /// <summary>Removes a key (no-op when absent or storage is unavailable).</summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            var module = await ModuleAsync();
            await module.InvokeVoidAsync("remove", key);
        }
        catch (JSException)
        {
            // ignore
        }
    }

    /// <summary>
    /// Reads and deserialises a JSON value. Returns <c>default</c> when the key is absent or the
    /// stored data is invalid/incompatible, so a corrupt entry never blocks startup.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, JsonSerializerOptions? options = null)
    {
        var json = await GetStringAsync(key);
        if (string.IsNullOrWhiteSpace(json)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>Serialises and writes a JSON value. Best-effort (see <see cref="SetStringAsync"/>).</summary>
    public async Task SetAsync<T>(string key, T value, JsonSerializerOptions? options = null) =>
        await SetStringAsync(key, JsonSerializer.Serialize(value, options));

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is { IsCompletedSuccessfully: true })
        {
            try
            {
                await (await _moduleTask).DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already gone — nothing to dispose.
            }
        }
    }
}
