import { defineConfig, devices } from '@playwright/test';

/**
 * De applicatie draait via docker compose. Deze configuratie start hem niet zelf:
 * `docker compose up -d --build` moet al gedraaid hebben.
 */
export default defineConfig({
  testDir: './specs',
  timeout: 30_000,
  expect: { timeout: 5_000 },
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never', outputFolder: 'playwright-report' }]],
  use: {
    baseURL: process.env.BASE_URL ?? 'http://localhost:3003',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'off',
    locale: 'nl-NL',
    timezoneId: 'Europe/Amsterdam'
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ]
});
