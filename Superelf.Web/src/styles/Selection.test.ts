/// <reference types="node" />
import { readFileSync } from 'node:fs';
import { blend, contrast } from '../testing/contrast';

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

// Gradient text is painted with the gradient's colours, so every stop must stay readable
// on the page and modal backgrounds and on the translucent white panels laid over them.
test('gradient text in Selection.css keeps at least 4.5:1 contrast on its surfaces', () => {
  const surfaces = ['#181c15', '#20251c', blend('#20251c', '#ffffff', 0.1)];
  const css = selectionCss.replace(/\/\*[\s\S]*?\*\//g, '');
  const textRules = [...css.matchAll(/([^{}]+)\{([^}]*background-clip:\s*text[^}]*)\}/g)];
  expect(textRules.map(([, selector]) => selector.trim())).toEqual(expect.arrayContaining([
    '.status-value', '.reserves-title', '.reserve-title', '.selection-container .modal-title',
  ]));

  const failures = textRules.flatMap(([, selector, body]) =>
    [...body.matchAll(/#[0-9a-fA-F]{6}\b/g)].flatMap(([stop]) =>
      surfaces
        .filter(surface => contrast(stop, surface) < 4.5)
        .map(surface => `${selector.trim()}: ${stop} on ${surface}`)
    )
  );
  expect(failures).toEqual([]);
});
