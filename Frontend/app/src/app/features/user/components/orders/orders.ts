import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { OrderService } from '../../services/order.service';
import { ReviewService } from '../../services/review.service';
import { InvoiceService } from '../../services/invoice.service';
import { PaginationModel } from '../../../../shared/models/users/pagination.model';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { toast } from 'ngx-sonner';
import { isOrderCancellable, isOrderDelivered, OrderStatus } from '../../../../constants/order-status.constants';
import { DEFAULT_PAGE_SIZE, calculateTotalPages } from '../../../../constants/pagination.constants';
import { OrderDetailsResponseDTO } from '../../../../shared/models/admin/orders.model';

interface ReviewData {
  productId: string;
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
  private invoiceService = inject(InvoiceService);

  orders = signal<OrderDetailsResponseDTO[]>([]);
  selectedOrder = signal<OrderDetailsResponseDTO | null>(null);
  showAddress = signal(false);
  showProducts = signal(false);
  isLoading = signal(false);

  currentPage = signal(1);
  pageSize = DEFAULT_PAGE_SIZE;
  totalItems = signal(0);
  totalPages = signal(0);

  showCancelModal = signal(false);
  orderToCancel = signal<OrderDetailsResponseDTO | null>(null);

  showReviewModal = signal(false);
  orderToReview = signal<OrderDetailsResponseDTO | null>(null);
  selectedProductId = signal<string>('');
  reviewSummary = signal('');
  reviewRating = signal(0);
  reviewHoverRating = signal(0);

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
        console.log(response)
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
    if (!order) return; const toastId = toast.loading('Cancelling order...');

    this.orderService.cancelOrder(order.orderId).subscribe({
      next: (response) => {
        toast.dismiss(toastId);
        toast.success(response.message || 'Order cancelled successfully');
        this.orders.update((orders) =>
          orders.map((o) =>
            o.orderId === order.orderId
              ? { ...o, status: OrderStatus.CANCELLED, isRefunded: false }
              : o
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

  canReviewOrder(order: OrderDetailsResponseDTO): boolean {
    return isOrderDelivered(order.status);
  }

  canDownloadInvoice(order: OrderDetailsResponseDTO): boolean {
    return isOrderDelivered(order.status);
  }

  showInvoicePreview = signal(false);
  invoiceOrder = signal<OrderDetailsResponseDTO | null>(null);
  invoiceDate = new Date().toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });

  openInvoicePreview(order: OrderDetailsResponseDTO): void {
    this.invoiceOrder.set(order);
    this.showInvoicePreview.set(true);
  }

  closeInvoicePreview(): void {
    this.showInvoicePreview.set(false);
    this.invoiceOrder.set(null);
  }

  downloadInvoice(order: OrderDetailsResponseDTO): void {
    try {
      this.invoiceService.download(order);
      toast.success('Invoice downloaded');
    } catch {
      toast.error('Failed to generate invoice');
    }
  }

  getLineTotal(price: number, qty: number): number {
    return price * qty;
  }

  hasReview(orderId: string): boolean {
    const order = this.orders().find(o => o.orderId === orderId);
    if (!order) return false;
    return order.items.every(item => this.hasProductReview(item.productId));
  }

  hasProductReview(productId: string): boolean {
    return this.reviews().some(r => r.productId === productId);
  }


  openReviewModal(order: OrderDetailsResponseDTO): void {
    const hasUnreviewed = order.items.some(i => !this.hasProductReview(i.productId));
    if (!hasUnreviewed) {
      return;
    }

    this.orderToReview.set(order);
    const firstUnreviewed = order.items.find(i => !this.hasProductReview(i.productId));
    this.selectedProductId.set(firstUnreviewed?.productId ?? order.items[0]?.productId ?? '');
    this.reviewSummary.set('');
    this.reviewRating.set(0);
    this.reviewHoverRating.set(0);
    this.showReviewModal.set(true);
  }

  closeReviewModal(): void {
    this.showReviewModal.set(false);
    this.orderToReview.set(null);
    this.selectedProductId.set('');
    this.reviewSummary.set('');
    this.reviewRating.set(0);
    this.reviewHoverRating.set(0);
  }

  submitReview(): void {
    const order = this.orderToReview();
    if (!order) {
      return;
    }

    if (!this.selectedProductId()) {
      toast.error('Please select a product to review');
      return;
    }

    if (!this.reviewSummary().trim()) {
      toast.error('Please enter a review summary');
      return;
    }

    if (this.reviewRating() === 0) {
      toast.error('Please select a rating');
      return;
    }

    const toastId = toast.loading('Submitting review...');
    this.reviewService.addReview(this.selectedProductId(), this.reviewSummary().trim(), this.reviewRating()).subscribe({
      next: (res) => {
        toast.dismiss(toastId);
        if (res.data?.reviewId) {
          toast.success('Review submitted successfully');
          this.reviews.update(r => [...r, {
            productId: this.selectedProductId(),
            summary: this.reviewSummary().trim(),
            rating: this.reviewRating(),
          }]);
          this.closeReviewModal();
        } else {
          toast.error(res.message || 'Failed to submit review');
        }
      },
      error: (err) => {
        console.error(err?.error?.message);
        toast.dismiss(toastId);
        toast.error(err?.error?.message || err.error.errors.Summary[0] || 'Failed to submit review');
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

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'not delivered': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'shipped': return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'delivered': return 'bg-green-100 text-green-800 border-green-200';
      case 'cancelled': return 'bg-red-100 text-red-800 border-red-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
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
