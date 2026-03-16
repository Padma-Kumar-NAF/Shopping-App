import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Order } from '../../../models/admin.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-orders-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders-management.html',
  styleUrl: './orders-management.css',
})
export class OrdersManagement {
  orders = signal<Order[]>([
    {
      id: '1',
      orderId: 'ORD-2024-001',
      customerName: 'John Doe',
      customerEmail: 'john@example.com',
      products: [
        {
          id: '1',
          name: 'Wireless Headphones',
          quantity: 2,
          price: 2999,
          image: 'https://picsum.photos/100/100?random=1',
        },
        {
          id: '2',
          name: 'Smart Watch',
          quantity: 1,
          price: 8999,
          image: 'https://picsum.photos/100/100?random=2',
        },
      ],
      totalPrice: 14997,
      orderDate: new Date('2024-03-10'),
      status: 'pending',
    },
    {
      id: '2',
      orderId: 'ORD-2024-002',
      customerName: 'Jane Smith',
      customerEmail: 'jane@example.com',
      products: [
        {
          id: '3',
          name: 'Laptop',
          quantity: 1,
          price: 45999,
          image: 'https://picsum.photos/100/100?random=3',
        },
      ],
      totalPrice: 45999,
      orderDate: new Date('2024-03-09'),
      status: 'processing',
    },
    {
      id: '3',
      orderId: 'ORD-2024-003',
      customerName: 'Bob Johnson',
      customerEmail: 'bob@example.com',
      products: [
        {
          id: '4',
          name: 'Running Shoes',
          quantity: 1,
          price: 3499,
          image: 'https://picsum.photos/100/100?random=4',
        },
      ],
      totalPrice: 3499,
      orderDate: new Date('2024-03-08'),
      status: 'shipped',
    },
    {
      id: '4',
      orderId: 'ORD-2024-004',
      customerName: 'Alice Williams',
      customerEmail: 'alice@example.com',
      products: [
        {
          id: '5',
          name: 'Coffee Maker',
          quantity: 1,
          price: 5999,
          image: 'https://picsum.photos/100/100?random=5',
        },
      ],
      totalPrice: 5999,
      orderDate: new Date('2024-03-07'),
      status: 'delivered',
    },
  ]);

  searchTerm = signal<string>('');
  filterStatus = signal<string>('all');
  expandedOrderId = signal<string | null>(null);

  get filteredOrders(): Order[] {
    let filtered = this.orders();

    // Search filter
    if (this.searchTerm()) {
      const term = this.searchTerm().toLowerCase();
      filtered = filtered.filter(
        (order) =>
          order.orderId.toLowerCase().includes(term) ||
          order.customerName.toLowerCase().includes(term) ||
          order.customerEmail.toLowerCase().includes(term)
      );
    }

    // Status filter
    if (this.filterStatus() !== 'all') {
      filtered = filtered.filter((order) => order.status === this.filterStatus());
    }

    return filtered;
  }

  updateSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
  }

  updateStatusFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterStatus.set(value);
  }

  changeOrderStatus(orderId: string, newStatus: Order['status']): void {
    const updatedOrders = this.orders().map((order) =>
      order.id === orderId ? { ...order, status: newStatus } : order
    );
    this.orders.set(updatedOrders);
    toast.success(`Order status updated to ${newStatus}`);
  }

  toggleOrderDetails(orderId: string): void {
    this.expandedOrderId.set(this.expandedOrderId() === orderId ? null : orderId);
  }

  getStatusColor(status: Order['status']): string {
    switch (status) {
      case 'pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'processing':
        return 'bg-blue-100 text-blue-800';
      case 'shipped':
        return 'bg-purple-100 text-purple-800';
      case 'delivered':
        return 'bg-green-100 text-green-800';
      case 'cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }
}
