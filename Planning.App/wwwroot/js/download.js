// Triggers a browser download of an in-memory file. Used by the Export menu.

export function downloadText(filename, text, mime) {
    downloadBlob(filename, new Blob([text], { type: mime || 'application/octet-stream' }));
}

// For future binary exports (e.g. SQLite): pass a base64 string.
export function downloadBytes(filename, base64, mime) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
    downloadBlob(filename, new Blob([bytes], { type: mime || 'application/octet-stream' }));
}

function downloadBlob(filename, blob) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
}
