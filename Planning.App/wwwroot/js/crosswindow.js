// Cross-tab/window notification bus for same-origin windows, backed by BroadcastChannel.
// Used to tell other open windows that a key in IndexedDB storage (see storage.js) has changed,
// so they can reload it and refresh their view. BroadcastChannel never delivers a message back
// to the window that posted it, so the .NET side needs no self-filtering.

const CHANNEL_NAME = 'planning-sync';
let channel;

function getChannel() {
    channel ??= new BroadcastChannel(CHANNEL_NAME);
    return channel;
}

export function subscribe(dotNetRef) {
    getChannel().onmessage = event => dotNetRef.invokeMethodAsync('ReceiveMessage', event.data);
}

export function post(key) {
    getChannel().postMessage(key);
}
