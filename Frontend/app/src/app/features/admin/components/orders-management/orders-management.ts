import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toast } from 'ngx-sonner';
import { StoreService } from '../../../services/adminServices/store.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  OrderDetailsResponseDTO,
  OrderRefundRequestDTO,
  OrderRefundResponseDTO,
} from '../../../models/admin/orders.model';
import { AdminOrderService } from '../../../services/adminServices/orders.service';
import { ApiResponse } from '../../../models/users/apiResponse.model';
import { GetAllOrderResponseDTO } from '../../../models/admin/orders.model';
import { PaginationModel } from '../../../models/users/pagination.model';

@Component({
  selector: 'app-orders-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders-management.html',
  styleUrl: './orders-management.css',
})

export class OrdersManagement implements OnInit {
  private readonly apiService = inject(AdminOrderService);
  store = inject(StoreService);

  orders$!: Observable<OrderDetailsResponseDTO[]>;

  searchTerm = signal<string>('');
  filterStatus = signal<string>('all');
  expandedOrderId = signal<string | null>(null);

  statusModal = signal<boolean>(false);
  pendingStatusOrderId = signal<string>('');
  selectedStatus = signal<string>('');

  currentPage = signal<number>(1);
  readonly pageSize = 10;

  hasMoreData = signal<boolean>(true);
  isLoading = signal<boolean>(false);

  pagination: PaginationModel;

  readonly statusOptions = ['Not Delivered', 'Shipped', 'Delivered'];

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = this.pageSize;
  }

  ngOnInit(): void {
    this.orders$ = this.store.state$.pipe(map((s) => s.orders));

    if (this.store.value.orders.length === 0) {
      this.fetchPage(1);
    }
  }

  private applyFilters(orders: OrderDetailsResponseDTO[]): OrderDetailsResponseDTO[] {
    const term = this.searchTerm().toLowerCase();
    return orders.filter((o) => {
      const matchesTerm =
        !term ||
        o.orderId.toLowerCase().includes(term) ||
        o.orderBy.userName.toLowerCase().includes(term) ||
        o.orderBy.userEmail.toLowerCase().includes(term);
      const matchesStatus = this.filterStatus() === 'all' || o.status === this.filterStatus();
      return matchesTerm && matchesStatus;
    });
  }

  get pagedOrders(): OrderDetailsResponseDTO[] {
    const filtered = this.applyFilters(this.store.value.orders);
    const start = (this.currentPage() - 1) * this.pageSize;
    return filtered.slice(start, start + this.pageSize);
  }

  get totalFiltered(): number {
    return this.applyFilters(this.store.value.orders).length;
  }

  get totalPages(): number {
    const fromStore = Math.max(1, Math.ceil(this.totalFiltered / this.pageSize));
    return this.hasMoreData() ? fromStore + 1 : fromStore;
  }

  giveRefund(order: OrderDetailsResponseDTO) {
    const request: OrderRefundRequestDTO = {
      OrderId: order.orderId,
      PaymentId: order.payment.paymentId,
      TotalAmount: order.totalAmount,
    };

    this.apiService.refundOrder(request).subscribe({
      next: (response: ApiResponse<OrderRefundResponseDTO>) => {
        toast.success(response.message ?? 'Refund Successful');
        this.store.setOrders(
          this.store.value.orders.map((o) =>
            o.orderId === order.orderId ? { ...o, isRefunded: true } : o,
          ),
        );
      },
      error: (err) => {
        console.error(err);
        toast.error(err?.error?.message || 'Failed to process refund');
        this.isLoading.set(false);
      },
      complete: () => {
        console.log('giveRefund completed');
      },
    });
  }

  getPageNumbers(): number[] {
    const total = Math.max(1, Math.ceil(this.totalFiltered / this.pageSize));
    const current = this.currentPage();
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    const range: number[] = [];
    for (let i = start; i <= end; i++) range.push(i);
    return range;
  }

  goToPage(page: number): void {
    if (page < 1 || this.isLoading()) return;

    const alreadyFetched = this.store.pageCache.orders.has(page);
    const dataExistsForPage =
      this.store.value.orders.length >= page * this.pageSize || alreadyFetched;

    if (dataExistsForPage) {
      this.currentPage.set(page);
      return;
    }

    this.fetchPage(page);
  }

  prevPage(): void {
    if (this.currentPage() <= 1 || this.isLoading()) return;
    this.currentPage.update((p) => p - 1);
  }

  nextPage(): void {
    if (this.isLoading()) return;
    this.goToPage(this.currentPage() + 1);
  }

  private fetchPage(page: number): void {
    this.isLoading.set(true);
    this.pagination.pageNumber = page;
    this.pagination.pageSize = this.pageSize;

    this.apiService.getAllOrders(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllOrderResponseDTO>) => {
        const incoming = response.data?.items ?? [];

        if (incoming.length === 0) {
          this.hasMoreData.set(false);
          toast.info('No more orders to load');
        } else {
          this.store.appendOrders(incoming);
          this.store.pageCache.orders.add(page);
          this.currentPage.set(page);

          if (incoming.length < this.pageSize) {
            this.hasMoreData.set(false);
          }
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        toast.error(err?.error?.message || 'Failed to load orders');
        this.isLoading.set(false);
      },
      complete: () => {
        console.log('getAllOrders completed');
      },
    });
  }

  private refreshFilter(): void {
    this.currentPage.set(1);
  }

  updateSearch(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
    this.refreshFilter();
  }

  updateStatusFilter(event: Event): void {
    this.filterStatus.set((event.target as HTMLSelectElement).value);
    this.refreshFilter();
  }

  toggleOrderDetails(orderId: string): void {
    this.expandedOrderId.set(this.expandedOrderId() === orderId ? null : orderId);
  }

  openStatusModal(orderId: string, currentStatus: string): void {
    this.pendingStatusOrderId.set(orderId);
    this.selectedStatus.set(currentStatus);
    this.statusModal.set(true);
  }

  cancelStatusChange(): void {
    this.statusModal.set(false);
    this.pendingStatusOrderId.set('');
    this.selectedStatus.set('');
  }

  confirmStatusChange(): void {
    const orderId = this.pendingStatusOrderId();
    const newStatus = this.selectedStatus();
    if (!orderId || !newStatus) return;

    this.apiService.updateOrder(orderId, newStatus).subscribe({
      next: () => {
        toast.success(`Order status updated to ${newStatus}`);
        this.store.setOrders(
          this.store.value.orders.map((o) =>
            o.orderId === orderId ? { ...o, status: newStatus } : o,
          ),
        );
        this.cancelStatusChange();
      },
      error: (err) => toast.error(err?.error?.message ?? 'Try again!'),
    });
  }

  setSelectedStatus(status: string): void {
    this.selectedStatus.set(status);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Not Delivered':
        return 'bg-yellow-100 text-yellow-800';
      case 'Shipped':
        return 'bg-purple-100 text-purple-800';
      case 'Delivered':
        return 'bg-green-100 text-green-800';
      case 'Cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }
}