// Copies plain text to the system clipboard. Used by the Conflicts panel.
// Returns true on success, false if the clipboard API is unavailable or blocked.
export async function copyText(text) {
    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return true;
        }
    } catch (e) {
        // Fall through to the legacy path below.
    }

    // Fallback for insecure contexts / older browsers.
    try {
        const area = document.createElement('textarea');
        area.value = text;
        area.style.position = 'fixed';
        area.style.opacity = '0';
        document.body.appendChild(area);
        area.focus();
        area.select();
        const ok = document.execCommand('copy');
        area.remove();
        return ok;
    } catch (e) {
        return false;
    }
}
