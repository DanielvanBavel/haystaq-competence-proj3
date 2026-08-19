import { useCallback, useEffect, useState } from 'react';
import { api, ApiError } from '../api';
import { OrderView, RestaurantSummary } from '../types';

const NEXT_LABEL: Record<string, string> = {
  Placed: 'Accepteren',
  Accepted: 'In de keuken',
  Preparing: 'Onderweg',
  OnTheWay: 'Bezorgd'
};

export function Admin() {
  const [restaurants, setRestaurants] = useState<RestaurantSummary[]>([]);
  const [restaurantId, setRestaurantId] = useState('');
  const [orders, setOrders] = useState<OrderView[]>([]);
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    api.get<RestaurantSummary[]>('/restaurants')
      .then((list) => {
        setRestaurants(list);
        setRestaurantId((current) => current || (list[0]?.id ?? ''));
      })
      .catch(() => setRestaurants([]));
  }, []);

  const load = useCallback(async () => {
    if (!restaurantId) {
      return;
    }
    try {
      setOrders(await api.get<OrderView[]>(`/restaurants/${restaurantId}/orders`));
    } catch (problem) {
      setMessage(problem instanceof ApiError ? problem.message : String(problem));
    }
  }, [restaurantId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function advance(order: OrderView, status?: string) {
    setMessage(null);
    try {
      await api.post(`/orders/${order.id}/advance`, { status: status ?? null, note: null });
      await load();
    } catch (problem) {
      setMessage(problem instanceof ApiError ? problem.message : String(problem));
    }
  }

  return (
    <section>
      <h1>Restaurantbeheer</h1>
      <label>
        Restaurant
        <select value={restaurantId} data-testid="admin-restaurant"
                onChange={(event) => setRestaurantId(event.target.value)}>
          {restaurants.map((restaurant) => (
            <option key={restaurant.id} value={restaurant.id}>{restaurant.name}</option>
          ))}
        </select>
      </label>

      <button type="button" onClick={() => void load()}>Verversen</button>
      {message ? <p className="error" data-testid="admin-message">{message}</p> : null}

      <table data-testid="admin-orders">
        <thead>
        <tr>
          <th>Bestelling</th>
          <th>Klant</th>
          <th>Bezorging</th>
          <th>Totaal</th>
          <th>Status</th>
          <th/>
        </tr>
        </thead>
        <tbody>
        {orders.map((order) => (
          <tr key={order.id}>
            <td>{order.orderNumber}</td>
            <td>{order.customerName}</td>
            <td>{order.deliveryDate} {order.deliverySlot}</td>
            <td>{order.total.toFixed(2)}</td>
            <td data-testid={`status-${order.orderNumber}`}>{order.status}</td>
            <td className="admin-actions">
              {NEXT_LABEL[order.status] ? (
                <button type="button" data-testid={`advance-${order.orderNumber}`}
                        onClick={() => void advance(order)}>
                  {NEXT_LABEL[order.status]}
                </button>
              ) : null}
              {order.status === 'Placed' ? (
                <button type="button" className="secondary"
                        onClick={() => void advance(order, 'Rejected')}>
                  Afwijzen
                </button>
              ) : null}
            </td>
          </tr>
        ))}
        </tbody>
      </table>

      {orders.length === 0 ? <p className="muted">Nog geen bestellingen.</p> : null}
    </section>
  );
}
