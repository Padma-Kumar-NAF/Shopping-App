import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import {
  OrderService,
  OrderDetailsResponseDTO,
} from '../../../services/userServices/order.service';
import { ReviewService } from '../../../services/review.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { toast } from 'ngx-sonner';
import { isOrderCancellable, isOrderDelivered, OrderStatus } from '../../../constants/order-status.constants';
import { DEFAULT_PAGE_SIZE, calculateTotalPages } from '../../../constants/pagination.constants';

interface ReviewData {
  orderId: string;
  summary: string;
  rating: number;
}

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule, PaginationComponent],
  templateUrl: './orders.html',
  styleUrls: ['./orders.css'],
})
export class OrdersComponent implements OnInit {
  private orderService = inject(OrderService);
  private reviewService = inject(ReviewService);

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
  cancelReason = signal('');

  // Review modal
  showReviewModal = signal(false);
  orderToReview = signal<OrderDetailsResponseDTO | null>(null);
  reviewSummary = signal('');
  reviewRating = signal(0);
  reviewHoverRating = signal(0);

  // Local reviews store (frontend only)
  reviews = signal<ReviewData[]>([]);

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

  // ── Cancel ──────────────────────────────────────────────────────

  canCancelOrder(order: OrderDetailsResponseDTO): boolean {
    return isOrderCancellable(order.status);
  }

  openCancelModal(order: OrderDetailsResponseDTO): void {
    this.orderToCancel.set(order);
    this.cancelReason.set('');
    this.showCancelModal.set(true);
  }

  closeCancelModal(): void {
    this.showCancelModal.set(false);
    this.orderToCancel.set(null);
    this.cancelReason.set('');
  }

  confirmCancelOrder(): void {
    const order = this.orderToCancel();
    if (!order) return;

    if (!this.cancelReason().trim()) {
      toast.error('Please enter a reason for cancellation');
      return;
    }

    const toastId = toast.loading('Cancelling order...');

    this.orderService.cancelOrder(order.orderId).subscribe({
      next: (response) => {
        toast.dismiss(toastId);
        toast.success(response.message || 'Order cancelled successfully');
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

  // ── Review ──────────────────────────────────────────────────────

  canReviewOrder(order: OrderDetailsResponseDTO): boolean {
    return isOrderDelivered(order.status);
  }

  hasReview(orderId: string): boolean {
    return this.reviews().some((r) => r.orderId === orderId);
  }

  getReview(orderId: string): ReviewData | undefined {
    return this.reviews().find((r) => r.orderId === orderId);
  }

  openReviewModal(order: OrderDetailsResponseDTO): void {
    const existing = this.getReview(order.orderId);
    this.orderToReview.set(order);
    this.reviewSummary.set(existing?.summary ?? '');
    this.reviewRating.set(existing?.rating ?? 0);
    this.reviewHoverRating.set(0);
    this.showReviewModal.set(true);
  }

  closeReviewModal(): void {
    this.showReviewModal.set(false);
    this.orderToReview.set(null);
    this.reviewSummary.set('');
    this.reviewRating.set(0);
    this.reviewHoverRating.set(0);
  }

  submitReview(): void {
    const order = this.orderToReview();
    if (!order) return;

    if (!this.reviewSummary().trim()) {
      toast.error('Please enter a review summary');
      return;
    }
    if (this.reviewRating() === 0) {
      toast.error('Please select a rating');
      return;
    }

    // Submit review for each product in the order
    const firstProduct = order.items?.[0];
    if (!firstProduct) { toast.error('No product found in order'); return; }

    const toastId = toast.loading('Submitting review...');
    this.reviewService.addReview(firstProduct.productId, this.reviewSummary().trim(), this.reviewRating()).subscribe({
      next: (res) => {
        toast.dismiss(toastId);
        if (res.data?.reviewId) {
          toast.success('Review submitted successfully');
          this.closeReviewModal();
        } else {
          toast.error(res.message || 'Failed to submit review');
        }
      },
      error: (err) => {
        toast.dismiss(toastId);
        toast.error(err?.error?.message || 'Failed to submit review');
      },
    });
  }

  setRating(value: number): void {
    this.reviewRating.set(value);
  }

  setHoverRating(value: number): void {
    this.reviewHoverRating.set(value);
  }

  getStarClass(star: number): string {
    const active = this.reviewHoverRating() || this.reviewRating();
    return star <= active ? 'text-yellow-400' : 'text-gray-300';
  }

  // ── Helpers ─────────────────────────────────────────────────────

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'not delivered': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'shipped':       return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'delivered':     return 'bg-green-100 text-green-800 border-green-200';
      case 'cancelled':     return 'bg-red-100 text-red-800 border-red-200';
      default:              return 'bg-gray-100 text-gray-800 border-gray-200';
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
    if (name.toLowerCase().includes('iphone') || name.toLowerCase().includes('phone')) return 'smartphone';
    if (name.toLowerCase().includes('macbook') || name.toLowerCase().includes('laptop')) return 'laptop';
    if (name.toLowerCase().includes('airpods') || name.toLowerCase().includes('headphones')) return 'headphones';
    return 'shopping_bag';
  }
}
