export interface User {
  id: string;
  name: string;
  email: string;
  role: 'admin' | 'customer';
  status: 'active' | 'inactive' | 'suspended';
  createdAt: Date;
}

export interface Order {
  id: string;
  orderId: string;
  customerName: string;
  customerEmail: string;
  products: OrderProduct[];
  totalPrice: number;
  orderDate: Date;
  status: 'pending' | 'processing' | 'shipped' | 'delivered' | 'cancelled';
}

export interface OrderProduct {
  id: string;
  name: string;
  quantity: number;
  price: number;
  image: string;
}

export interface Product {
  id?: string;
  name: string;
  category: string;
  quantity: number;
  imageUrl: string;
  price: number;
  description?: string;
  createdAt?: Date;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
  productCount?: number;
  createdAt: Date;
}

export interface DashboardStats {
  totalUsers: number;
  totalOrders: number;
  totalProducts: number;
  totalRevenue: number;
  pendingOrders: number;
  activeUsers: number;
}
