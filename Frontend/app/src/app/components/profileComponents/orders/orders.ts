import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import {
  OrderService,
  OrderDetailsResponseDTO,
} from '../../../services/userServices/order.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './orders.html',
  styleUrls: ['./orders.css'],
})
export class OrdersComponent implements OnInit {
  private orderService = inject(OrderService);

  orders = signal<OrderDetailsResponseDTO[]>([]);
  selectedOrder = signal<OrderDetailsResponseDTO | null>(null);
  showAddress = signal(false);
  showProducts = signal(false);
  isLoading = signal(false);

  currentPage = signal(1);
  pageSize = 10;

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading.set(true);
    const pagination = new PaginationModel();
    pagination.pageNumber = this.currentPage();
    pagination.pageSize = this.pageSize;

    this.orderService.getUserOrders(pagination).subscribe({
      next: (response) => {
        if (response.data?.items) {
          this.orders.set(response.data.items);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load orders:', err);
        toast.error(err?.error?.message || 'Failed to load orders');
        this.isLoading.set(false);
      },
    });
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'not delivered':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'shipped':
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'delivered':
        return 'bg-green-100 text-green-800 border-green-200';
      case 'cancelled':
        return 'bg-red-100 text-red-800 border-red-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  viewAddress(order: OrderDetailsResponseDTO): void {
    this.selectedOrder.set(order);
    this.showAddress.set(true);
    this.showProducts.set(false);
  }

  viewProducts(order: OrderDetailsResponseDTO): void {
    this.selectedOrder.set(order);
    this.showProducts.set(true);
    this.showAddress.set(false);
  }

  closeDetails(): void {
    this.showAddress.set(false);
    this.showProducts.set(false);
    this.selectedOrder.set(null);
  }

  getProductIcon(name: string): string {
    if (name.toLowerCase().includes('iphone') || name.toLowerCase().includes('phone'))
      return 'smartphone';
    if (name.toLowerCase().includes('macbook') || name.toLowerCase().includes('laptop'))
      return 'laptop';
    if (name.toLowerCase().includes('airpods') || name.toLowerCase().includes('headphones'))
      return 'headphones';
    return 'shopping_bag';
  }
}
