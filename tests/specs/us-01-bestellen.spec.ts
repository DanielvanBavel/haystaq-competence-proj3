import { expect, test } from '../support/fixtures';
import { CheckoutPage, HomePage, RestaurantPage } from '../support/pages';

/**
 * US-01: Als hongerige bezoeker wil ik een maaltijd bestellen en betalen,
 * zodat mijn eten wordt bezorgd.
 *
 * Dit is de gouden voorbeeldtest: gebruik hem als maatstaf voor de tests die je
 * laat genereren. Let op de opzet: page objects, wachten op gedrag in plaats van
 * op tijd, en controles op wat de gebruiker daadwerkelijk ziet.
 */
test('US-01 bezoeker bestelt een pizza en rekent af met iDEAL', async ({ page }) => {
  const home = new HomePage(page);
  const restaurant = new RestaurantPage(page);
  const checkout = new CheckoutPage(page);

  await home.open();
  await home.search('Pizzeria');
  await home.openRestaurant('Pizzeria De Vuurplaat');

  await expect(page.getByRole('heading', { name: 'Pizzeria De Vuurplaat' })).toBeVisible();

  await restaurant.addItem('Margherita', { size: 'Groot', extras: ['Extra kaas'], quantity: 2 });
  await expect(page.getByTestId('cart-count')).toHaveText('2');

  await checkout.open();
  await expect(checkout.total()).toHaveText('30.50');

  await checkout.fillCustomer('Sanne de Wit', 'sanne@example.com');
  await checkout.fillAddress('Ginnekenweg', '12A', '4818 JD', 'Breda');
  await checkout.choosePaymentMethod('Ideal');
  await checkout.payWithIdeal('ING');
  await checkout.placeOrder();

  await expect(page).toHaveURL(/\/bestelling\/BB-\d{4}-\d{5}/);
  await expect(page.getByTestId('order-status')).toHaveText('Ontvangen');
  await expect(page.getByTestId('order-total')).toHaveText('30.50');
});
