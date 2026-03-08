import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface Order {
  id: string;
  status: 'pending' | 'shipped' | 'delivered' | 'cancelled';
  deliveryDate?: string;
  address: {
    name: string;
    line1: string;
    line2?: string;
    city: string;
    state: string;
    pincode: string;
  };
  products: Array<{
    name: string;
    image: string;
    quantity: number;
    price: number;
  }>;
}

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './orders.html',
  styleUrls: ['./orders.css']
})
export class OrdersComponent {
  orders = signal<Order[]>([
    {
      id: '1',
      status: 'delivered',
      deliveryDate: '2026-02-15',
      address: {
        name: 'Padma Kumar',
        line1: '123 MG Road',
        line2: 'Anna Nagar',
        city: 'Chennai',
        state: 'Tamil Nadu',
        pincode: '600040'
      },
      products: [
        { name: 'iPhone 16 Pro', image: '/assets/iphone.jpg', quantity: 1, price: 999 },
        { name: 'AirPods Pro 2', image: '/assets/airpods.jpg', quantity: 1, price: 249 }
      ]
    },
    {
      id: '2',
      status: 'shipped',
      deliveryDate: '2026-03-12',
      address: {
        name: 'Padma Kumar',
        line1: '456 Mount Road',
        city: 'Chennai',
        state: 'Tamil Nadu',
        pincode: '600002'
      },
      products: [
        { name: 'MacBook Air M3', image: '/assets/macbook.jpg', quantity: 1, price: 1299 }
      ]
    },
    {
      id: '3',
      status: 'pending',
      address: {
        name: 'Padma Kumar',
        line1: '789 T Nagar',
        city: 'Chennai',
        state: 'Tamil Nadu',
        pincode: '600017'
      },
      products: [
        { name: 'Samsung Galaxy S25', image: '/assets/samsung.jpg', quantity: 1, price: 899 }
      ]
    }
  ]);

  selectedOrder = signal<Order | null>(null);
  showAddress = signal(false);
  showProducts = signal(false);

  statusColor = {
    pending: 'bg-yellow-100 text-yellow-800 border-yellow-200',
    shipped: 'bg-blue-100 text-blue-800 border-blue-200',
    delivered: 'bg-green-100 text-green-800 border-green-200',
    cancelled: 'bg-red-100 text-red-800 border-red-200'
  };

  viewAddress(order: Order) {
    this.selectedOrder.set(order);
    this.showAddress.set(true);
    this.showProducts.set(false);
  }

  viewProducts(order: Order) {
    this.selectedOrder.set(order);
    this.showProducts.set(true);
    this.showAddress.set(false);
  }

  closeDetails() {
    this.showAddress.set(false);
    this.showProducts.set(false);
    this.selectedOrder.set(null);
  }

  getProductIcon(name: string): string {
    if (name.toLowerCase().includes('iphone') || name.toLowerCase().includes('phone')) return 'smartphone';
    if (name.toLowerCase().includes('macbook') || name.toLowerCase().includes('laptop')) return 'laptop';
    if (name.toLowerCase().includes('airpods') || name.toLowerCase().includes('headphones')) return 'headphones';
    return 'shopping_bag';
  }
}
