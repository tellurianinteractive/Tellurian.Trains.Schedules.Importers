// Measures how wide text is in a given font, so a report can work out how many lines a note wraps onto
// in the font the planner actually chose instead of guessing from a character count.
//
// The measurement is the browser's own (canvas measureText), made at a large size and scaled down so
// that rounding to whole pixels cannot distort the small sizes reports are set in.

const MeasureSize = 200;
const MillimetresPerPoint = 25.4 / 72;

/**
 * Returns the width in millimetres of each word, followed by the width of a single space, when set in
 * `fontFamily` at `sizePt` points. Returns an empty array when the measurement cannot be made, so the
 * caller falls back to its own estimate rather than trusting zeros.
 * @param {string} fontFamily a CSS font-family list
 * @param {number} sizePt font size in points
 * @param {string[]} words the words to measure
 * @returns {number[]} one width per word, in order, then the width of a space
 */
export function measure(fontFamily, sizePt, words) {
    const context = document.createElement('canvas').getContext('2d');
    if (!context || !Array.isArray(words)) return [];

    context.font = `${MeasureSize}px ${fontFamily}`;
    const scale = (sizePt * MillimetresPerPoint) / MeasureSize;
    const widths = words.map(word => context.measureText(word).width * scale);
    widths.push(context.measureText(' ').width * scale);
    return widths;
}
