import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api';
import { MenuItem, RestaurantDetail } from '../types';
import { useCart } from '../CartContext';

export function Restaurant() {
  const { slug } = useParams();
  const cart = useCart();
  const [detail, setDetail] = useState<RestaurantDetail | null>(null);
  const [selected, setSelected] = useState<MenuItem | null>(null);
  const [chosenOptions, setChosenOptions] = useState<string[]>([]);
  const [quantity, setQuantity] = useState(1);
  const [toast, setToast] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!slug) {
      return;
    }
    try {
      setDetail(await api.get<RestaurantDetail>(`/restaurants/${slug}`));
    } catch {
      setError('Dit restaurant bestaat niet.');
    }
  }, [slug]);

  useEffect(() => {
    void load();
  }, [load]);

  // De melding na toevoegen verdwijnt vanzelf.
  useEffect(() => {
    if (!toast) {
      return;
    }
    const timer = window.setTimeout(() => setToast(null), 3000);
    return () => window.clearTimeout(timer);
  }, [toast]);

  function openItem(item: MenuItem) {
    setSelected(item);
    setQuantity(1);
    setChosenOptions(item.options.filter((option) => option.isDefault).map((option) => option.id));
  }

  function toggleOption(optionId: string, kind: 'Size' | 'Extra') {
    setChosenOptions((current) => {
      if (kind === 'Size') {
        const others = current.filter(
          (id) => selected?.options.find((option) => option.id === id)?.kind !== 'Size'
        );
        return [...others, optionId];
      }
      return current.includes(optionId)
        ? current.filter((id) => id !== optionId)
        : [...current, optionId];
    });
  }

  function addToCart() {
    if (!selected || !detail) {
      return;
    }
    const optionTotal = selected.options
      .filter((option) => chosenOptions.includes(option.id))
      .reduce((total, option) => total + option.priceDelta, 0);
    const summary = selected.options
      .filter((option) => chosenOptions.includes(option.id))
      .map((option) => option.name)
      .join(', ');

    cart.add(detail.restaurant.id, detail.restaurant.name, {
      menuItemId: selected.id,
      itemName: selected.name,
      optionIds: chosenOptions,
      optionSummary: summary,
      quantity,
      unitPrice: Number((selected.price + optionTotal).toFixed(2))
    });

    setToast(`${quantity}x ${selected.name} toegevoegd`);
    setSelected(null);
  }

  if (error) {
    return <p className="error">{error}</p>;
  }
  if (!detail) {
    return <p className="muted">Bezig met laden...</p>;
  }

  const categories = Array.from(new Set(detail.menu.map((item) => item.category)));

  return (
    <section>
      <h1>{detail.restaurant.name}</h1>
      <p className="muted">
        {detail.restaurant.cuisine} &middot; {detail.restaurant.city} &middot;{' '}
        minimaal {detail.restaurant.minimumOrder.toFixed(2)} &middot;{' '}
        bezorgkosten {detail.restaurant.deliveryFee.toFixed(2)}
      </p>
      {!detail.restaurant.isOpen ? (
        <p className="notice" data-testid="closed-notice">
          Dit restaurant is op dit moment gesloten. Je kunt wel alvast rondkijken.
        </p>
      ) : null}

      {categories.map((category) => (
        <div key={category}>
          <h2>{category}</h2>
          <ul className="menu">
            {detail.menu.filter((item) => item.category === category).map((item) => (
              <li key={item.id} className={item.isAvailable ? 'menu-item' : 'menu-item unavailable'}>
                <div>
                  <h3>{item.name}{item.isVegetarian ? ' (v)' : ''}</h3>
                  {item.description ? <p className="muted">{item.description}</p> : null}
                  {!item.isAvailable ? <p className="badge closed-badge">Uitverkocht</p> : null}
                </div>
                <div className="menu-actions">
                  <span className="price">{item.price.toFixed(2)}</span>
                  <button
                    type="button"
                    disabled={!item.isAvailable}
                    onClick={() => openItem(item)}
                  >
                    Toevoegen
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </div>
      ))}

      {toast ? <div className="toast" data-testid="cart-toast">{toast}</div> : null}

      {selected ? (
        <div className="modal-backdrop" role="dialog" aria-modal="true" data-testid="item-dialog">
          <div className="modal">
            <h2>{selected.name}</h2>
            {selected.options.filter((option) => option.kind === 'Size').length > 0 ? (
              <fieldset>
                <legend>Maat</legend>
                {selected.options.filter((option) => option.kind === 'Size').map((option) => (
                  <label key={option.id} className="check">
                    <input
                      type="radio"
                      name="size"
                      checked={chosenOptions.includes(option.id)}
                      onChange={() => toggleOption(option.id, 'Size')}
                    />
                    {option.name} {option.priceDelta > 0 ? `(+${option.priceDelta.toFixed(2)})` : ''}
                  </label>
                ))}
              </fieldset>
            ) : null}

            {selected.options.filter((option) => option.kind === 'Extra').length > 0 ? (
              <fieldset>
                <legend>Extra's</legend>
                {selected.options.filter((option) => option.kind === 'Extra').map((option) => (
                  <label key={option.id} className="check">
                    <input
                      type="checkbox"
                      checked={chosenOptions.includes(option.id)}
                      onChange={() => toggleOption(option.id, 'Extra')}
                    />
                    {option.name} {option.priceDelta > 0 ? `(+${option.priceDelta.toFixed(2)})` : ''}
                  </label>
                ))}
              </fieldset>
            ) : null}

            <label>
              Aantal
              <input
                type="number"
                min={1}
                max={20}
                value={quantity}
                data-testid="item-quantity"
                onChange={(event) => setQuantity(Number(event.target.value))}
              />
            </label>

            <div className="modal-actions">
              <button type="button" onClick={() => setSelected(null)} className="secondary">Annuleren</button>
              <button type="button" onClick={addToCart} data-testid="confirm-add">In winkelmandje</button>
            </div>
          </div>
        </div>
      ) : null}
    </section>
  );
}
