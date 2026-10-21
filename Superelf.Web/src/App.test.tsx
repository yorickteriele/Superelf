import React from 'react';
import { render, screen } from '@testing-library/react';
import App from './App';

test('renders the navbar brand', () => {
  render(<App />);
  const brandLink = screen.getByRole('link', { name: /superelf/i });
  expect(brandLink).toBeInTheDocument();
});
