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
