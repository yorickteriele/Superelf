/// <reference types="node" />
import { readFileSync } from 'node:fs';

// Read from disk: Vitest strips CSS content, even for ?raw imports.
const selectionCss = readFileSync(`${__dirname}/Selection.css`, 'utf8');

// Selection.css is bundled globally, so overrides of Bootstrap classes must be
// scoped to the selection page or they restyle every other page too.
const bootstrapClasses = [
  'btn', 'btn-outline-light', 'btn-primary', 'btn-success', 'btn-warning',
  'progress-bar', 'modal-content', 'modal-header', 'modal-title',
  'alert', 'alert-error', 'alert-success', 'spinner-border',
  'text-primary', 'visually-hidden', 'text-center', 'd-none',
];

test('Selection.css does not override Bootstrap classes globally', () => {
  const unscoped = bootstrapClasses.filter((name) =>
    new RegExp(`^\\s*\\.${name}(?![\\w-])`, 'm').test(selectionCss)
  );
  expect(unscoped).toEqual([]);
});
