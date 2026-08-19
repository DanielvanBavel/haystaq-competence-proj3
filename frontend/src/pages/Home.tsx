import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { RestaurantSummary } from '../types';

export function Home() {
  const [restaurants, setRestaurants] = useState<RestaurantSummary[]>([]);
  const [cuisines, setCuisines] = useState<string[]>([]);
  const [query, setQuery] = useState('');
  const [cuisine, setCuisine] = useState('');
  const [maxMinutes, setMaxMinutes] = useState('');
  const [onlyOpen, setOnlyOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.get<string[]>('/cuisines').then(setCuisines).catch(() => setCuisines([]));
  }, []);

  // Zoeken gaat pas na 300 ms zonder toetsaanslag naar de server.
  useEffect(() => {
    setLoading(true);
    const timer = window.setTimeout(() => {
      const params = new URLSearchParams();
      if (query.trim()) params.set('query', query.trim());
      if (cuisine) params.set('cuisine', cuisine);
      if (maxMinutes) params.set('maxDeliveryMinutes', maxMinutes);
      if (onlyOpen) params.set('onlyOpen', 'true');

      api.get<RestaurantSummary[]>(`/restaurants?${params.toString()}`)
        .then(setRestaurants)
        .catch(() => setRestaurants([]))
        .finally(() => setLoading(false));
    }, 300);

    return () => window.clearTimeout(timer);
  }, [query, cuisine, maxMinutes, onlyOpen]);

  return (
    <section>
      <h1>Wat wil je vanavond eten?</h1>

      <div className="filters">
        <label>
          Zoeken
          <input
            type="search"
            data-testid="restaurant-search"
            placeholder="Restaurant of keuken"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
          />
        </label>
        <label>
          Keuken
          <select value={cuisine} onChange={(event) => setCuisine(event.target.value)}>
            <option value="">Alle keukens</option>
            {cuisines.map((option) => (
              <option key={option} value={option}>{option}</option>
            ))}
          </select>
        </label>
        <label>
          Bezorgtijd
          <select value={maxMinutes} onChange={(event) => setMaxMinutes(event.target.value)}>
            <option value="">Maakt niet uit</option>
            <option value="30">Binnen 30 minuten</option>
            <option value="45">Binnen 45 minuten</option>
          </select>
        </label>
        <label className="check">
          <input type="checkbox" checked={onlyOpen} onChange={(event) => setOnlyOpen(event.target.checked)}/>
          Alleen open
        </label>
      </div>

      {loading ? <p className="muted">Bezig met laden...</p> : null}

      <ul className="restaurant-list" data-testid="restaurant-list">
        {restaurants.map((restaurant) => (
          <li key={restaurant.id} className={restaurant.isOpen ? 'restaurant' : 'restaurant closed'}>
            <Link to={`/restaurant/${restaurant.slug}`}>
              <div className="restaurant-head">
                <h2>{restaurant.name}</h2>
                <span className="rating">{restaurant.rating.toFixed(1)}</span>
              </div>
              <p className="muted">{restaurant.cuisine} &middot; {restaurant.city}</p>
              <p className="meta">
                Min. {restaurant.minimumOrder.toFixed(2)} &middot;{' '}
                bezorgkosten {restaurant.deliveryFee.toFixed(2)} &middot;{' '}
                {restaurant.estimatedDeliveryMinutes} min
              </p>
              {restaurant.freeDeliveryFrom !== null ? (
                <p className="badge">Gratis bezorgd vanaf {restaurant.freeDeliveryFrom.toFixed(2)}</p>
              ) : null}
              {!restaurant.isOpen ? <p className="badge closed-badge">Gesloten</p> : null}
            </Link>
          </li>
        ))}
      </ul>

      {!loading && restaurants.length === 0 ? (
        <p className="muted" data-testid="no-results">Geen restaurants gevonden.</p>
      ) : null}
    </section>
  );
}
