import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import {
  OrderService,
  OrderDetailsResponseDTO,
} from '../../../services/userServices/order.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { toast } from 'ngx-sonner';
import { isOrderCancellable, OrderStatus } from '../../../constants/order-status.constants';
import { DEFAULT_PAGE_SIZE, calculateTotalPages } from '../../../constants/pagination.constants';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, PaginationComponent],
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

  // Pagination
  currentPage = signal(1);
  pageSize = DEFAULT_PAGE_SIZE;
  totalItems = signal(0);
  totalPages = signal(0);

  // Cancel order modal
  showCancelModal = signal(false);
  orderToCancel = signal<OrderDetailsResponseDTO | null>(null);

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
          // Note: Backend should return total count, using items length as fallback
          this.totalItems.set(response.data.items.length);
          this.totalPages.set(calculateTotalPages(this.totalItems(), this.pageSize));
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

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadOrders();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  canCancelOrder(order: OrderDetailsResponseDTO): boolean {
    return isOrderCancellable(order.status);
  }

  openCancelModal(order: OrderDetailsResponseDTO): void {
    this.orderToCancel.set(order);
    this.showCancelModal.set(true);
  }

  closeCancelModal(): void {
    this.showCancelModal.set(false);
    this.orderToCancel.set(null);
  }

  confirmCancelOrder(): void {
    const order = this.orderToCancel();
    if (!order) return;

    const toastId = toast.loading('Cancelling order...');

    this.orderService.cancelOrder(order.orderId).subscribe({
      next: (response) => {
        toast.dismiss(toastId);
        toast.success(response.message || 'Order cancelled successfully');

        // Update order status in the list
        this.orders.update((orders) =>
          orders.map((o) =>
            o.orderId === order.orderId ? { ...o, status: OrderStatus.CANCELLED } : o
          )
        );

        this.closeCancelModal();
      },
      error: (err) => {
        toast.dismiss(toastId);
        toast.error(err?.error?.message || 'Failed to cancel order');
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
