// Narrows a list of font families to the ones this device can actually render, so the report font
// setting offers real choices rather than names that would silently fall back to something else.
//
// The test is the long-established measurement one: draw the same sample in "<family>, <generic>" and
// in "<generic>" alone. A missing family falls back to the generic and the two widths match exactly;
// an installed one almost always renders at a different width. All three generics are tried, because
// a family that happens to measure the same as one of them will differ from the others.
//
// There is a browser API that enumerates installed fonts (queryLocalFonts), but it is Chromium-only
// and raises a permission prompt, so it is deliberately not used: measurement needs no permission and
// works in every browser.

const Sample = 'WMlim0123456789 Kobenhavn';
const Size = '72px';
const Generics = ['monospace', 'serif', 'sans-serif'];

/**
 * Returns the subset of `families` that is installed on this device.
 * If the measurement cannot be made at all, every family is returned rather than none — an empty
 * list would leave the planner with nothing to choose from, which is worse than offering a font
 * that turns out to fall back.
 * @param {string[]} families family names to test
 * @returns {string[]} those that are installed, in the order given
 */
export function installed(families) {
    if (!Array.isArray(families) || families.length === 0) return [];

    const context = document.createElement('canvas').getContext('2d');
    if (!context) return families;

    const baselines = Generics.map(generic => widthOf(context, `${Size} ${generic}`));
    return families.filter(family =>
        Generics.some((generic, i) => widthOf(context, `${Size} "${family}", ${generic}`) !== baselines[i]));
}

function widthOf(context, font) {
    context.font = font;
    return context.measureText(Sample).width;
}
