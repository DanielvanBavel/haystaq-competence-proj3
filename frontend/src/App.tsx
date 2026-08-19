import { Link, NavLink, Navigate, Route, Routes } from 'react-router-dom';
import { Home } from './pages/Home';
import { Restaurant } from './pages/Restaurant';
import { Checkout } from './pages/Checkout';
import { OrderStatus } from './pages/OrderStatus';
import { Admin } from './pages/Admin';
import { useCart } from './CartContext';

export function App() {
  const cart = useCart();

  return (
    <div className="app">
      <header>
        <Link to="/" className="brand">BezorgBaas</Link>
        <nav>
          <NavLink to="/" end className={({ isActive }) => (isActive ? 'tab selected' : 'tab')}>
            Restaurants
          </NavLink>
          <NavLink to="/beheer" className={({ isActive }) => (isActive ? 'tab selected' : 'tab')}>
            Restaurantbeheer
          </NavLink>
        </nav>
        <Link to="/afrekenen" className="cart-link" data-testid="cart-link">
          Winkelmandje
          <span className="cart-count" data-testid="cart-count">{cart.count}</span>
        </Link>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<Home/>}/>
          <Route path="/restaurant/:slug" element={<Restaurant/>}/>
          <Route path="/afrekenen" element={<Checkout/>}/>
          <Route path="/bestelling/:orderNumber" element={<OrderStatus/>}/>
          <Route path="/beheer" element={<Admin/>}/>
          <Route path="*" element={<Navigate to="/" replace/>}/>
        </Routes>
      </main>
    </div>
  );
}
