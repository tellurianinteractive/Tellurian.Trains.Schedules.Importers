export function startDrag(container, dotNetRef, orientation) {
    const firstPane = container.querySelector(':scope > .split-pane-first');
    if (!firstPane) return;

    const onPointerMove = (e) => {
        e.preventDefault();
        const rect = container.getBoundingClientRect();
        let ratio;
        if (orientation === 'horizontal') {
            ratio = (e.clientX - rect.left) / rect.width;
        } else {
            ratio = (e.clientY - rect.top) / rect.height;
        }
        ratio = Math.max(0.1, Math.min(0.9, ratio));
        const pct = (ratio * 100).toFixed(1) + '%';
        container.style.setProperty('--split-first', pct);
    };

    const onPointerUp = (e) => {
        document.removeEventListener('pointermove', onPointerMove);
        document.removeEventListener('pointerup', onPointerUp);
        document.body.style.cursor = '';
        document.body.style.userSelect = '';

        const rect = container.getBoundingClientRect();
        let ratio;
        if (orientation === 'horizontal') {
            ratio = (e.clientX - rect.left) / rect.width;
        } else {
            ratio = (e.clientY - rect.top) / rect.height;
        }
        ratio = Math.max(0.1, Math.min(0.9, ratio));
        dotNetRef.invokeMethodAsync('OnDragEnd', ratio);
    };

    document.addEventListener('pointermove', onPointerMove);
    document.addEventListener('pointerup', onPointerUp);
    document.body.style.cursor = orientation === 'horizontal' ? 'col-resize' : 'row-resize';
    document.body.style.userSelect = 'none';
}
