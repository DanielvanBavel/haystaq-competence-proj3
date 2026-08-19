import { expect, Locator, Page } from '@playwright/test';

/**
 * Page objects voor de schermen die de gouden voorbeeldtest gebruikt.
 * Gebruik dit als voorbeeld, niet als volledige bibliotheek: de meeste schermen
 * hebben nog geen page object.
 */
export class HomePage {
  constructor(private readonly page: Page) {
  }

  async open(): Promise<void> {
    await this.page.goto('/');
  }

  async search(term: string): Promise<void> {
    await this.page.getByTestId('restaurant-search').fill(term);
    // De zoekopdracht is gedebounced; wachten op het resultaat in plaats van op tijd.
    await expect(this.page.getByTestId('restaurant-list')).toBeVisible();
  }

  restaurant(name: string): Locator {
    return this.page.getByRole('link', { name: new RegExp(name, 'i') });
  }

  async openRestaurant(name: string): Promise<void> {
    await this.restaurant(name).first().click();
  }
}

export class RestaurantPage {
  constructor(private readonly page: Page) {
  }

  async addItem(itemName: string, options: { size?: string; extras?: string[]; quantity?: number } = {}) {
    const row = this.page.locator('.menu-item').filter({ hasText: itemName }).first();
    await row.getByRole('button', { name: 'Toevoegen' }).click();

    const dialog = this.page.getByTestId('item-dialog');
    await expect(dialog).toBeVisible();

    if (options.size) {
      await dialog.getByText(options.size, { exact: false }).click();
    }
    for (const extra of options.extras ?? []) {
      await dialog.getByText(extra, { exact: false }).click();
    }
    if (options.quantity) {
      await dialog.getByTestId('item-quantity').fill(String(options.quantity));
    }

    await dialog.getByTestId('confirm-add').click();
    await expect(this.page.getByTestId('cart-toast')).toBeVisible();
  }
}

export class CheckoutPage {
  constructor(private readonly page: Page) {
  }

  async open(): Promise<void> {
    await this.page.getByTestId('cart-link').click();
  }

  async fillCustomer(name: string, email: string): Promise<void> {
    await this.page.getByTestId('customer-name').fill(name);
    await this.page.getByTestId('customer-email').fill(email);
  }

  async fillAddress(street: string, houseNumber: string, postalCode: string, city: string): Promise<void> {
    await this.page.getByTestId('address-street').fill(street);
    await this.page.getByTestId('address-house-number').fill(houseNumber);
    await this.page.getByTestId('address-postal-code').fill(postalCode);
    await this.page.getByTestId('address-city').fill(city);
  }

  async applyPromo(code: string): Promise<void> {
    await this.page.getByTestId('promo-input').fill(code);
    await this.page.getByTestId('promo-apply').click();
  }

  async choosePaymentMethod(method: 'Ideal' | 'Card' | 'Cash'): Promise<void> {
    await this.page.getByTestId('payment-method').selectOption(method);
  }

  /** De betaling gebeurt in een iframe, net als bij een echte betaaldienst. */
  async payWithIdeal(bank: string): Promise<void> {
    const frame = this.page.frameLocator('[data-testid="payment-iframe"]');
    await frame.getByTestId('payment-bank').selectOption(bank);
    await frame.getByTestId('payment-submit').click();
    await expect(this.page.getByTestId('payment-feedback')).toContainText('bevestigd');
  }

  async payWithCard(cardNumber: string): Promise<void> {
    const frame = this.page.frameLocator('[data-testid="payment-iframe"]');
    await frame.getByTestId('payment-card-number').fill(cardNumber);
    await frame.getByTestId('payment-submit').click();
  }

  async placeOrder(): Promise<void> {
    await this.page.getByTestId('place-order').click();
  }

  total(): Locator {
    return this.page.getByTestId('total');
  }
}
