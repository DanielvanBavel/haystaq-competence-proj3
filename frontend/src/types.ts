export interface RestaurantSummary {
  id: string;
  slug: string;
  name: string;
  cuisine: string;
  city: string;
  rating: number;
  estimatedDeliveryMinutes: number;
  minimumOrder: number;
  deliveryFee: number;
  freeDeliveryFrom: number | null;
  isOpen: boolean;
}

export interface MenuItemOption {
  id: string;
  name: string;
  kind: 'Size' | 'Extra';
  priceDelta: number;
  isDefault: boolean;
}

export interface MenuItem {
  id: string;
  name: string;
  description: string | null;
  category: string;
  price: number;
  isAvailable: boolean;
  isVegetarian: boolean;
  spicinessLevel: number;
  options: MenuItemOption[];
}

export interface RestaurantDetail {
  restaurant: RestaurantSummary;
  menu: MenuItem[];
}

export interface QuoteLine {
  menuItemId: string;
  itemName: string;
  optionSummary: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Quote {
  restaurantId: string;
  restaurantName: string;
  lines: QuoteLine[];
  subtotal: number;
  deliveryFee: number;
  discount: number;
  total: number;
  minimumOrder: number;
  meetsMinimum: boolean;
  promoCode: string | null;
  promoMessage: string | null;
}

export interface OrderLineView {
  itemName: string;
  optionSummary: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderStatusView {
  status: string;
  note: string | null;
  changedAt: string;
}

export interface OrderView {
  id: string;
  orderNumber: string;
  restaurantId: string;
  restaurantName: string;
  customerName: string;
  customerEmail: string;
  address: string;
  deliveryDate: string;
  deliverySlot: string;
  paymentMethod: string;
  status: string;
  subtotal: number;
  deliveryFee: number;
  discount: number;
  total: number;
  promoCode: string | null;
  placedAt: string;
  lines: OrderLineView[];
  history: OrderStatusView[];
}
