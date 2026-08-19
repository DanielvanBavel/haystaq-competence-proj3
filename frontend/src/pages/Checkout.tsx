import { FormEvent, useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, ApiError } from '../api';
import { OrderView, Quote } from '../types';
import { useCart } from '../CartContext';

const TODAY = new Date().toISOString().slice(0, 10);

export function Checkout() {
  const cart = useCart();
  const navigate = useNavigate();

  const [quote, setQuote] = useState<Quote | null>(null);
  const [promoInput, setPromoInput] = useState('');
  const [appliedPromo, setAppliedPromo] = useState<string | null>(null);
  const [slots, setSlots] = useState<string[]>([]);
  const [paymentReference, setPaymentReference] = useState<string | null>(null);
  const [paymentMessage, setPaymentMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const [form, setForm] = useState({
    customerName: '',
    customerEmail: '',
    street: '',
    houseNumber: '',
    postalCode: '',
    city: '',
    note: '',
    deliveryDate: TODAY,
    deliverySlot: '',
    paymentMethod: 'Ideal'
  });

  const refreshQuote = useCallback(async () => {
    if (!cart.restaurantId || cart.items.length === 0) {
      setQuote(null);
      return;
    }
    try {
      const result = await api.post<Quote>('/orders/quote', {
        restaurantId: cart.restaurantId,
        lines: cart.items.map((item) => ({
          menuItemId: item.menuItemId,
          quantity: item.quantity,
          optionIds: item.optionIds
        })),
        promoCode: appliedPromo,
        customerEmail: form.customerEmail || null
      });
      setQuote(result);
    } catch (problem) {
      setError(problem instanceof ApiError ? problem.message : String(problem));
    }
  }, [cart.restaurantId, cart.items, appliedPromo, form.customerEmail]);

  useEffect(() => {
    void refreshQuote();
  }, [refreshQuote]);

  useEffect(() => {
    api.get<string[]>(`/delivery-slots?date=${form.deliveryDate}`)
      .then((result) => {
        setSlots(result);
        setForm((current) => ({
          ...current,
          deliverySlot: result.includes(current.deliverySlot) ? current.deliverySlot : (result[0] ?? '')
        }));
      })
      .catch(() => setSlots([]));
  }, [form.deliveryDate]);

  // De betaal-iframe meldt het resultaat via postMessage.
  useEffect(() => {
    function onMessage(event: MessageEvent) {
      const data = event.data as { type?: string; reference?: string; message?: string };
      if (data?.type === 'payment-approved') {
        setPaymentReference(data.reference ?? null);
        setPaymentMessage('Betaling bevestigd.');
      }
      if (data?.type === 'payment-declined') {
        setPaymentReference(null);
        setPaymentMessage(data.message ?? 'De betaling is geweigerd.');
      }
    }
    window.addEventListener('message', onMessage);
    return () => window.removeEventListener('message', onMessage);
  }, []);

  useEffect(() => {
    setPaymentReference(null);
    setPaymentMessage(null);
  }, [form.paymentMethod]);

  function update(field: string, value: string) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const order = await api.post<OrderView>('/orders', {
        restaurantId: cart.restaurantId,
        lines: cart.items.map((item) => ({
          menuItemId: item.menuItemId,
          quantity: item.quantity,
          optionIds: item.optionIds
        })),
        customerName: form.customerName,
        customerEmail: form.customerEmail,
        address: {
          street: form.street,
          houseNumber: form.houseNumber,
          postalCode: form.postalCode,
          city: form.city,
          note: form.note || null
        },
        deliveryDate: form.deliveryDate,
        deliverySlot: form.deliverySlot,
        paymentMethod: form.paymentMethod,
        paymentReference,
        promoCode: appliedPromo
      });
      cart.clear();
      navigate(`/bestelling/${order.orderNumber}`);
    } catch (problem) {
      setError(problem instanceof ApiError ? problem.message : String(problem));
    } finally {
      setBusy(false);
    }
  }

  if (cart.items.length === 0) {
    return (
      <section>
        <h1>Afrekenen</h1>
        <p className="muted" data-testid="empty-cart">Je winkelmandje is leeg.</p>
      </section>
    );
  }

  const needsPayment = form.paymentMethod !== 'Cash';

  return (
    <section className="checkout">
      <h1>Afrekenen bij {cart.restaurantName}</h1>

      <div className="checkout-grid">
        <form onSubmit={submit} data-testid="checkout-form">
          <h2>Je gegevens</h2>
          <label>
            Naam
            <input required value={form.customerName} data-testid="customer-name"
                   onChange={(event) => update('customerName', event.target.value)}/>
          </label>
          <label>
            E-mailadres
            <input required type="email" value={form.customerEmail} data-testid="customer-email"
                   onChange={(event) => update('customerEmail', event.target.value)}/>
          </label>

          <h2>Bezorgadres</h2>
          <div className="row">
            <label>
              Straat
              <input required value={form.street} data-testid="address-street"
                     onChange={(event) => update('street', event.target.value)}/>
            </label>
            <label>
              Huisnummer
              <input required value={form.houseNumber} data-testid="address-house-number"
                     onChange={(event) => update('houseNumber', event.target.value)}/>
            </label>
          </div>
          <div className="row">
            <label>
              Postcode
              <input required placeholder="4811 AB" value={form.postalCode} data-testid="address-postal-code"
                     onChange={(event) => update('postalCode', event.target.value)}/>
            </label>
            <label>
              Plaats
              <input required value={form.city} data-testid="address-city"
                     onChange={(event) => update('city', event.target.value)}/>
            </label>
          </div>
          <label>
            Opmerking voor de bezorger
            <input value={form.note} onChange={(event) => update('note', event.target.value)}/>
          </label>

          <h2>Bezorgmoment</h2>
          <div className="row">
            <label>
              Datum
              <input type="date" min={TODAY} value={form.deliveryDate} data-testid="delivery-date"
                     onChange={(event) => update('deliveryDate', event.target.value)}/>
            </label>
            <label>
              Tijdvak
              <select value={form.deliverySlot} data-testid="delivery-slot"
                      onChange={(event) => update('deliverySlot', event.target.value)}>
                {slots.map((slot) => (
                  <option key={slot} value={slot}>{slot}</option>
                ))}
              </select>
            </label>
          </div>

          <h2>Betalen</h2>
          <label>
            Betaalmethode
            <select value={form.paymentMethod} data-testid="payment-method"
                    onChange={(event) => update('paymentMethod', event.target.value)}>
              <option value="Ideal">iDEAL</option>
              <option value="Card">Creditcard</option>
              <option value="Cash">Contant bij de deur</option>
            </select>
          </label>

          {needsPayment ? (
            <div className="payment-frame">
              <iframe
                title="Betaalscherm"
                data-testid="payment-iframe"
                src={`/payment-mock.html?method=${form.paymentMethod.toLowerCase()}&amount=${quote?.total ?? 0}`}
                height={form.paymentMethod === 'Card' ? 260 : 240}
              />
            </div>
          ) : (
            <p className="muted">Je betaalt contant aan de bezorger.</p>
          )}

          {paymentMessage ? <p className="notice" data-testid="payment-feedback">{paymentMessage}</p> : null}
          {error ? <p className="error" data-testid="checkout-error">{error}</p> : null}

          <button type="submit" disabled={busy} data-testid="place-order">
            {busy ? 'Bezig...' : 'Bestelling plaatsen'}
          </button>
        </form>

        <aside className="summary" data-testid="order-summary">
          <h2>Je bestelling</h2>
          <ul>
            {cart.items.map((item, index) => (
              <li key={`${item.menuItemId}-${index}`}>
                <div>
                  <strong>{item.quantity}x {item.itemName}</strong>
                  {item.optionSummary ? <p className="muted">{item.optionSummary}</p> : null}
                </div>
                <div className="line-actions">
                  <span>{(item.unitPrice * item.quantity).toFixed(2)}</span>
                  <button type="button" className="link" onClick={() => cart.remove(index)}>verwijderen</button>
                </div>
              </li>
            ))}
          </ul>

          <div className="promo">
            <label>
              Actiecode
              <input value={promoInput} data-testid="promo-input"
                     onChange={(event) => setPromoInput(event.target.value)}/>
            </label>
            <button type="button" data-testid="promo-apply"
                    onClick={() => setAppliedPromo(promoInput.trim().toUpperCase() || null)}>
              Toepassen
            </button>
          </div>
          {quote?.promoMessage ? (
            <p className="notice" data-testid="promo-message">{quote.promoMessage}</p>
          ) : null}

          {quote ? (
            <dl className="totals">
              <div><dt>Subtotaal</dt><dd data-testid="subtotal">{quote.subtotal.toFixed(2)}</dd></div>
              <div><dt>Bezorgkosten</dt><dd data-testid="delivery-fee">{quote.deliveryFee.toFixed(2)}</dd></div>
              {quote.discount > 0 ? (
                <div><dt>Korting</dt><dd data-testid="discount">-{quote.discount.toFixed(2)}</dd></div>
              ) : null}
              <div className="grand"><dt>Totaal</dt><dd data-testid="total">{quote.total.toFixed(2)}</dd></div>
            </dl>
          ) : null}

          {quote && !quote.meetsMinimum ? (
            <p className="error" data-testid="minimum-warning">
              Het minimale bestelbedrag is {quote.minimumOrder.toFixed(2)}.
            </p>
          ) : null}
        </aside>
      </div>
    </section>
  );
}
