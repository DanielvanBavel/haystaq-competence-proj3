import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api, ApiError } from '../api';
import { OrderView } from '../types';

const LABELS: Record<string, string> = {
  Placed: 'Ontvangen',
  Accepted: 'Geaccepteerd',
  Preparing: 'In de keuken',
  OnTheWay: 'Onderweg',
  Delivered: 'Bezorgd',
  Cancelled: 'Geannuleerd',
  Rejected: 'Afgewezen'
};

const FLOW = ['Placed', 'Accepted', 'Preparing', 'OnTheWay', 'Delivered'];

export function OrderStatus() {
  const { orderNumber } = useParams();
  const [order, setOrder] = useState<OrderView | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!orderNumber) {
      return;
    }
    try {
      setOrder(await api.get<OrderView>(`/orders/${orderNumber}`));
    } catch (problem) {
      setError(problem instanceof ApiError ? problem.message : String(problem));
    }
  }, [orderNumber]);

  useEffect(() => {
    void load();
    // De statuspagina ververst zichzelf, net als in het echt.
    const timer = window.setInterval(() => void load(), 5000);
    return () => window.clearInterval(timer);
  }, [load]);

  async function cancel() {
    if (!order) {
      return;
    }
    try {
      await api.post(`/orders/${order.id}/cancel`, { reason: 'Klant heeft geannuleerd' });
      setMessage('Je bestelling is geannuleerd.');
      await load();
    } catch (problem) {
      setMessage(problem instanceof ApiError ? problem.message : String(problem));
    }
  }

  if (error) {
    return <p className="error" data-testid="order-error">{error}</p>;
  }
  if (!order) {
    return <p className="muted">Bezig met laden...</p>;
  }

  const currentIndex = FLOW.indexOf(order.status);

  return (
    <section>
      <h1>Bestelling {order.orderNumber}</h1>
      <p className="muted">{order.restaurantName} &middot; bezorging {order.deliveryDate} tussen {order.deliverySlot}</p>

      <ol className="tracker" data-testid="status-tracker">
        {FLOW.map((status, index) => (
          <li key={status} className={index <= currentIndex ? 'done' : ''}>
            {LABELS[status]}
          </li>
        ))}
      </ol>

      <p className="status-line">
        Status: <strong data-testid="order-status">{LABELS[order.status] ?? order.status}</strong>
      </p>

      {order.status === 'Placed' || order.status === 'Accepted' ? (
        <button type="button" onClick={() => void cancel()} data-testid="cancel-order">
          Bestelling annuleren
        </button>
      ) : null}
      {message ? <p className="notice" data-testid="cancel-message">{message}</p> : null}

      <h2>Overzicht</h2>
      <table data-testid="order-lines">
        <tbody>
        {order.lines.map((line, index) => (
          <tr key={index}>
            <td>{line.quantity}x</td>
            <td>
              {line.itemName}
              {line.optionSummary ? <div className="muted">{line.optionSummary}</div> : null}
            </td>
            <td className="right">{line.lineTotal.toFixed(2)}</td>
          </tr>
        ))}
        <tr>
          <td/>
          <td>Bezorgkosten</td>
          <td className="right">{order.deliveryFee.toFixed(2)}</td>
        </tr>
        {order.discount > 0 ? (
          <tr>
            <td/>
            <td>Korting {order.promoCode ? `(${order.promoCode})` : ''}</td>
            <td className="right">-{order.discount.toFixed(2)}</td>
          </tr>
        ) : null}
        <tr className="grand">
          <td/>
          <td>Totaal</td>
          <td className="right" data-testid="order-total">{order.total.toFixed(2)}</td>
        </tr>
        </tbody>
      </table>

      <h2>Verloop</h2>
      <ul className="history">
        {order.history.map((change, index) => (
          <li key={index}>
            <span className="muted">{new Date(change.changedAt).toLocaleTimeString('nl-NL')}</span>{' '}
            {LABELS[change.status] ?? change.status}
            {change.note ? ` - ${change.note}` : ''}
          </li>
        ))}
      </ul>
    </section>
  );
}
