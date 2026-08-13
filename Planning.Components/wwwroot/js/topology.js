// Dragging an operation location in the topology diagram.
//
// The diagram is laid out in SVG user units and a moved location is saved in those same units, but a
// pointer arrives in screen pixels — and the two are not proportional, because the SVG is scaled to the
// width it is given. getScreenCTM() is what relates them, so every position reported back to .NET is put
// through its inverse first. What is reported is the movement since the pointer went down, not where the
// pointer is: that keeps the location under the part of the circle it was grabbed by instead of jumping
// its centre to the pointer.

export function startDrag(svg, dotNetRef, clientX, clientY) {
    const point = svg.createSVGPoint();

    const toUser = (x, y) => {
        const matrix = svg.getScreenCTM();
        if (!matrix) return null;
        point.x = x;
        point.y = y;
        return point.matrixTransform(matrix.inverse());
    };

    const origin = toUser(clientX, clientY);
    if (!origin) return;

    let pending = null;
    let frame = 0;

    // One report per animation frame. A pointer fires far more often than the diagram can be redrawn,
    // and every redraw crosses into .NET.
    const report = () => {
        frame = 0;
        if (!pending) return;
        const moved = pending;
        pending = null;
        dotNetRef.invokeMethodAsync('OnDragMove', moved.x - origin.x, moved.y - origin.y);
    };

    const onPointerMove = (e) => {
        e.preventDefault();
        const at = toUser(e.clientX, e.clientY);
        if (!at) return;
        pending = at;
        if (!frame) frame = requestAnimationFrame(report);
    };

    const onPointerUp = (e) => {
        document.removeEventListener('pointermove', onPointerMove);
        document.removeEventListener('pointerup', onPointerUp);
        document.removeEventListener('pointercancel', onPointerUp);
        if (frame) {
            cancelAnimationFrame(frame);
            frame = 0;
        }
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
        const at = toUser(e.clientX, e.clientY);
        dotNetRef.invokeMethodAsync('OnDragEnd', at ? at.x - origin.x : 0, at ? at.y - origin.y : 0);
    };

    document.addEventListener('pointermove', onPointerMove);
    document.addEventListener('pointerup', onPointerUp);
    document.addEventListener('pointercancel', onPointerUp);
    document.body.style.userSelect = 'none';
    document.body.style.cursor = 'grabbing';
}
