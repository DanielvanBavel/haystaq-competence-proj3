import { test as base, expect, request } from '@playwright/test';

const API = process.env.API_URL ?? 'http://localhost:8083';

/**
 * Elke test begint met dezelfde gegevens. Zonder deze reset zijn tests van
 * elkaar afhankelijk en gaan ze willekeurig falen.
 */
export const test = base.extend({
  page: async ({ page }, use) => {
    const context = await request.newContext();
    await context.post(`${API}/api/test-support/reset`);
    await context.dispose();
    await page.goto('/');
    await page.evaluate(() => window.localStorage.clear());
    await use(page);
  }
});

export { expect };
