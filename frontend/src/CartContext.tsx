import { createContext, ReactNode, useContext, useEffect, useMemo, useState } from 'react';

export interface CartItem {
  menuItemId: string;
  itemName: string;
  optionIds: string[];
  optionSummary: string;
  quantity: number;
  unitPrice: number;
}

interface CartState {
  restaurantId: string | null;
  restaurantName: string | null;
  items: CartItem[];
  count: number;
  add: (restaurantId: string, restaurantName: string, item: CartItem) => void;
  changeQuantity: (index: number, quantity: number) => void;
  remove: (index: number) => void;
  clear: () => void;
}

const STORAGE_KEY = 'bezorgbaas-cart';

const Context = createContext<CartState>({
  restaurantId: null,
  restaurantName: null,
  items: [],
  count: 0,
  add: () => undefined,
  changeQuantity: () => undefined,
  remove: () => undefined,
  clear: () => undefined
});

interface StoredCart {
  restaurantId: string | null;
  restaurantName: string | null;
  items: CartItem[];
}

function load(): StoredCart {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as StoredCart) : { restaurantId: null, restaurantName: null, items: [] };
  } catch {
    return { restaurantId: null, restaurantName: null, items: [] };
  }
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [cart, setCart] = useState<StoredCart>(load);

  useEffect(() => {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(cart));
  }, [cart]);

  const value = useMemo<CartState>(() => ({
    restaurantId: cart.restaurantId,
    restaurantName: cart.restaurantName,
    items: cart.items,
    count: cart.items.reduce((total, item) => total + item.quantity, 0),
    add: (restaurantId, restaurantName, item) => {
      setCart((current) => {
        // Je kunt maar bij één restaurant tegelijk bestellen.
        const sameRestaurant = current.restaurantId === restaurantId;
        const items = sameRestaurant ? [...current.items] : [];
        const existing = items.findIndex(
          (candidate) =>
            candidate.menuItemId === item.menuItemId &&
            candidate.optionIds.slice().sort().join(',') === item.optionIds.slice().sort().join(',')
        );
        if (existing >= 0) {
          items[existing] = { ...items[existing], quantity: items[existing].quantity + item.quantity };
        } else {
          items.push(item);
        }
        return { restaurantId, restaurantName, items };
      });
    },
    changeQuantity: (index, quantity) => {
      setCart((current) => {
        const items = [...current.items];
        if (quantity <= 0) {
          items.splice(index, 1);
        } else {
          items[index] = { ...items[index], quantity };
        }
        return { ...current, items };
      });
    },
    remove: (index) => {
      setCart((current) => {
        const items = [...current.items];
        items.splice(index, 1);
        return { ...current, items };
      });
    },
    clear: () => setCart({ restaurantId: null, restaurantName: null, items: [] })
  }), [cart]);

  return <Context.Provider value={value}>{children}</Context.Provider>;
}

export function useCart() {
  return useContext(Context);
}
