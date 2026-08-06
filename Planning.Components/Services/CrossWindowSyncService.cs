using Microsoft.JSInterop;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// Broadcasts and receives lightweight "this key changed" notifications between browser tabs/windows
/// on the same origin, via the JS <c>BroadcastChannel</c> API (see <c>_content/Tellurian.Trains.Schedules.Planning.Components/js/crosswindow.js</c>).
/// BroadcastChannel never delivers a message back to the window that posted it, so no self-filtering
/// is needed on the .NET side. Registered as a singleton; the JS module and channel are set up once
/// on first use. Best-effort: if BroadcastChannel is unavailable, sync is silently skipped.
/// </summary>
public sealed class CrossWindowSyncService(IJSRuntime js) : IAsyncDisposable
{
    private const string ModulePath = "./_content/Tellurian.Trains.Schedules.Planning.Components/js/crosswindow.js";
    private Task<IJSObjectReference>? _moduleTask;
    private DotNetObjectReference<CrossWindowSyncService>? _selfRef;
    private bool _subscribed;

    /// <summary>Raised when another window posts a message with the given key.</summary>
    public event Action<string>? OnMessage;

    private Task<IJSObjectReference> ModuleAsync() =>
        _moduleTask ??= js.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();

    /// <summary>Starts listening for messages from other windows. Call once; later calls are no-ops.</summary>
    public async Task SubscribeAsync()
    {
        if (_subscribed) return;
        _subscribed = true;
        try
        {
            var module = await ModuleAsync();
            _selfRef = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("subscribe", _selfRef);
        }
        catch (JSException)
        {
            // BroadcastChannel unsupported or unavailable — cross-window sync is best-effort.
        }
    }

    /// <summary>Notifies other windows that the data identified by <paramref name="key"/> has changed.</summary>
    public async Task PostAsync(string key)
    {
        try
        {
            var module = await ModuleAsync();
            await module.InvokeVoidAsync("post", key);
        }
        catch (JSException)
        {
            // Best-effort, same as above.
        }
    }

    [JSInvokable]
    public void ReceiveMessage(string key) => OnMessage?.Invoke(key);

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();
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
