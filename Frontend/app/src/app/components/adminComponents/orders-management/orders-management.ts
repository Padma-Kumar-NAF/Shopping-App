import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toast } from 'ngx-sonner';
import { StoreService } from '../../../services/adminServices/store.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { OrderDetailsResponseDTO, UpdateOrderResponseDTO } from '../../../models/admin/orders.model';
import { AdminOrderService } from '../../../services/adminServices/orders.service';
import { ApiResponse } from '../../../models/users/apiResponse.model';

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
  filteredOrders$!: Observable<OrderDetailsResponseDTO[]>;

  searchTerm = signal<string>('');
  filterStatus = signal<string>('all');

  expandedOrderId = signal<string | null>(null);

  statusModal = signal<boolean>(false);
  pendingStatusOrderId = signal<string>("");
  selectedStatus = signal<string>('');

  readonly statusOptions = ['Not Delivered', 'Shipped', 'Delivered', 'Cancelled'];

  ngOnInit(): void {
    this.orders$ = this.store.state$.pipe(map((s) => s.orders));
    this.filteredOrders$ = this.store.state$.pipe(
      map((s) => {
        let filtered = s.orders;

        const term = this.searchTerm().toLowerCase();
        if (term) {
          filtered = filtered.filter(
            (o) =>
              o.orderId.toLowerCase().includes(term) ||
              o.orderBy.userName.toLowerCase().includes(term) ||
              o.orderBy.userEmail.toLowerCase().includes(term),
          );
        }

        if (this.filterStatus() !== 'all') {
          filtered = filtered.filter((o) => o.status === this.filterStatus());
        }

        return filtered;
      }),
    );
  }

  private refreshFilter(): void {
    this.store.setOrders([...this.store.value.orders]);
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
    this.pendingStatusOrderId.set("");
    this.selectedStatus.set('');
  }

  confirmStatusChange(): void {
    const orderId = this.pendingStatusOrderId();
    const newStatus = this.selectedStatus();

    this.apiService.updateOrder(orderId,newStatus).subscribe({
      next : (response : ApiResponse<UpdateOrderResponseDTO>)=> {
        console.log("response")
        console.log(response)
        toast.success(`Order status updated to ${newStatus}`);
      },
      error: (err) => {
        console.error(err)
        console.log(err?.error?.message)
        toast.error(err?.error?.message ?? "Try again !")
      },
    })

    if (!orderId || !newStatus) return;

    const updatedOrders = this.store.value.orders.map((o) =>
      o.orderId === orderId ? { ...o, status: newStatus } : o,
    );
    this.store.setOrders(updatedOrders);

    toast.success(`Order status updated to ${newStatus}`);
    this.cancelStatusChange();
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
