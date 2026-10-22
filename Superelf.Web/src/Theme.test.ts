/// <reference types="node" />
import { readFileSync } from 'node:fs';

// Read from disk: Vitest strips CSS content, even for ?raw imports.
const appCss = readFileSync(`${__dirname}/App.css`, 'utf8');
const navbar = readFileSync(`${__dirname}/components/Navbar.tsx`, 'utf8');
const footer = readFileSync(`${__dirname}/components/Footer.tsx`, 'utf8');

// App.tsx puts data-bs-theme="dark" on the body, where Bootstrap redeclares its colour
// variables; overrides on :root alone are shadowed there and never reach the page.
test('App.css overrides Bootstrap colours where its dark theme declares them', () => {
  const themeBlock = appCss.match(/:root,\s*\[data-bs-theme=dark\]\s*\{([^}]*)\}/);
  expect(themeBlock).not.toBeNull();
  expect(themeBlock![1]).toMatch(/--bs-body-bg:/);
  expect(themeBlock![1]).toMatch(/--bs-primary:/);
});

test('Navbar and Footer take their colours from the theme, not inline hex values', () => {
  expect(navbar).not.toMatch(/#[0-9a-fA-F]{3,6}\b/);
  expect(footer).not.toMatch(/#[0-9a-fA-F]{3,6}\b/);
});

// WCAG relative luminance and contrast ratio for #rrggbb colours.
const luminance = (hex: string) => {
  const [r, g, b] = [1, 3, 5].map(i => {
    const c = parseInt(hex.slice(i, i + 2), 16) / 255;
    return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
  });
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
};
const contrast = (a: string, b: string) => {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
  return (hi + 0.05) / (lo + 0.05);
};

// The semantic colours are light on this theme; Bootstrap's white badge text is unreadable on them.
test('badges on semantic backgrounds use dark ink with at least 4.5:1 contrast', () => {
  const themeBlock = appCss.match(/:root,\s*\[data-bs-theme=dark\]\s*\{([^}]*)\}/)![1];
  const ink = themeBlock.match(/--se-accent-ink:\s*(#[0-9a-fA-F]{6})/)![1];
  const badgeRule = appCss.match(/\.badge:is\(([^)]*)\)\s*\{([^}]*)\}/);
  expect(badgeRule).not.toBeNull();
  expect(badgeRule![2]).toMatch(/--bs-badge-color:\s*var\(--se-accent-ink\)/);

  for (const name of ['success', 'info', 'warning', 'danger', 'secondary']) {
    const bg = themeBlock.match(new RegExp(`--bs-${name}:\\s*(#[0-9a-fA-F]{6})`))![1];
    expect(badgeRule![1]).toContain(`.bg-${name}`);
    expect(contrast(ink, bg)).toBeGreaterThanOrEqual(4.5);
  }
  const accent = themeBlock.match(/--se-accent:\s*(#[0-9a-fA-F]{6})/)![1];
  expect(badgeRule![1]).toContain('.bg-primary');
  expect(contrast(ink, accent)).toBeGreaterThanOrEqual(4.5);
});
