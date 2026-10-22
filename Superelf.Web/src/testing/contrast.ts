// WCAG relative luminance and contrast ratio for #rrggbb colours.
const channels = (hex: string) => [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16));

const luminance = (hex: string) => {
  const [r, g, b] = channels(hex).map(v => {
    const c = v / 255;
    return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
  });
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
};

export const contrast = (a: string, b: string) => {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
  return (hi + 0.05) / (lo + 0.05);
};

// The opaque colour of `top` laid over `base` at the given alpha.
export const blend = (base: string, top: string, alpha: number) =>
  '#' + channels(base)
    .map((v, i) => Math.round(v * (1 - alpha) + channels(top)[i] * alpha).toString(16).padStart(2, '0'))
    .join('');
