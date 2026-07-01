// Triggers a browser download of an in-memory file. Used by the Export menu.
//
// Two-step API for showSaveFilePicker:
//   1. openSaveFilePicker() — must be called while user activation is live (before any Task.Delay yield).
//      Returns true if picker opened, null if user cancelled, false if API not supported.
//   2. writePendingText(text) — write content after serialisation; awaits handle if dialog still open.

let _pendingHandle = null;

export async function openSaveFilePicker(filename, mime) {
    if (typeof window.showSaveFilePicker !== 'function') return false;
    const ext = filename.split('.').pop();
    try {
        _pendingHandle = await window.showSaveFilePicker({
            suggestedName: filename,
            types: [{ accept: { [mime]: ['.' + ext] } }],
        });
        return true;
    } catch (e) {
        _pendingHandle = null;
        return e.name === 'AbortError' ? null : false;
    }
}

export async function writePendingText(text) {
    const writable = await _pendingHandle.createWritable();
    await writable.write(text);
    await writable.close();
    _pendingHandle = null;
}

// Fallback: anchor-click download (Firefox, Safari, insecure contexts).
export function downloadText(filename, text, mime) {
    const blob = new Blob([text], { type: mime || 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
}

// For future binary exports (e.g. SQLite): pass a base64 string.
export function downloadBytes(filename, base64, mime) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
    const blob = new Blob([bytes], { type: mime || 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
}
