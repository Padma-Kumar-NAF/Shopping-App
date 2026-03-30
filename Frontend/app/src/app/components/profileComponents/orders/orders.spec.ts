import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { OrdersComponent } from './orders';
import { OrderService } from '../../../services/userServices/order.service';
import { ReviewService } from '../../../services/review.service';
import { InvoiceService } from '../../../services/invoice.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('OrdersComponent', () => {
  let component: OrdersComponent;
  let fixture: ComponentFixture<OrdersComponent>;

  const getUserOrders = vi.fn();
  const cancelOrder = vi.fn();
  const addReview = vi.fn();
  const download = vi.fn();

  const mockOrder: any = {
    orderId: 'o1',
    status: 'Delivered',
    totalAmount: 5000,
    isRefunded: false,
    items: [{ productId: 'p1', productName: 'Laptop', price: 5000, quantity: 1 }],
    orderBy: { userName: 'Alice', userEmail: 'alice@example.com' },
    payment: { paymentId: 'pay1' },
    address: { addressLine1: '123 Main St', city: 'Chennai' },
  };

  const mockPendingOrder: any = {
    ...mockOrder,
    orderId: 'o2',
    status: 'Not Delivered',
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    getUserOrders.mockReturnValue(of({ data: { items: [mockOrder, mockPendingOrder] } }));

    await TestBed.configureTestingModule({
      imports: [OrdersComponent],
      providers: [
        { provide: OrderService, useValue: { getUserOrders, cancelOrder } },
        { provide: ReviewService, useValue: { addReview } },
        { provide: InvoiceService, useValue: { download } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(OrdersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load orders on init', () => {
    expect(component.orders().length).toBe(2);
    expect(component.isLoading()).toBe(false);
  });

  it('should set isLoading to false on load failure', () => {
    getUserOrders.mockReturnValue(throwError(() => ({ error: { message: 'Error' } })));
    component.loadOrders();
    expect(component.isLoading()).toBe(false);
  });

  describe('onPageChange', () => {
    it('should update currentPage and reload orders', () => {
      component.onPageChange(2);
      expect(component.currentPage()).toBe(2);
      expect(getUserOrders).toHaveBeenCalledTimes(2);
    });
  });

  describe('canCancelOrder', () => {
    it('should return true for Not Delivered orders', () => {
      expect(component.canCancelOrder(mockPendingOrder)).toBe(true);
    });

    it('should return false for Delivered orders', () => {
      expect(component.canCancelOrder(mockOrder)).toBe(false);
    });
  });

  describe('Cancel modal', () => {
    it('openCancelModal should set orderToCancel and show modal', () => {
      component.openCancelModal(mockPendingOrder);
      expect(component.orderToCancel()).toEqual(mockPendingOrder);
      expect(component.showCancelModal()).toBe(true);
    });

    it('closeCancelModal should hide modal and clear orderToCancel', () => {
      component.openCancelModal(mockPendingOrder);
      component.closeCancelModal();
      expect(component.showCancelModal()).toBe(false);
      expect(component.orderToCancel()).toBeNull();
    });

    it('confirmCancelOrder should call cancelOrder and update status', () => {
      cancelOrder.mockReturnValue(of({ message: 'Cancelled' }));
      component.orderToCancel.set(mockPendingOrder);
      component.confirmCancelOrder();
      expect(cancelOrder).toHaveBeenCalledWith('o2');
      const updated = component.orders().find((o: any) => o.orderId === 'o2');
      expect(updated?.status).toBe('Cancelled');
    });
  });

  describe('canReviewOrder', () => {
    it('should return true for Delivered orders', () => {
      expect(component.canReviewOrder(mockOrder)).toBe(true);
    });

    it('should return false for non-Delivered orders', () => {
      expect(component.canReviewOrder(mockPendingOrder)).toBe(false);
    });
  });

  describe('canDownloadInvoice', () => {
    it('should return true for Delivered orders', () => {
      expect(component.canDownloadInvoice(mockOrder)).toBe(true);
    });
  });

  describe('Invoice', () => {
    it('openInvoicePreview should set invoiceOrder and show preview', () => {
      component.openInvoicePreview(mockOrder);
      expect(component.invoiceOrder()).toEqual(mockOrder);
      expect(component.showInvoicePreview()).toBe(true);
    });

    it('closeInvoicePreview should hide preview', () => {
      component.openInvoicePreview(mockOrder);
      component.closeInvoicePreview();
      expect(component.showInvoicePreview()).toBe(false);
    });

    it('downloadInvoice should call invoiceService.download', () => {
      component.downloadInvoice(mockOrder);
      expect(download).toHaveBeenCalledWith(mockOrder);
    });
  });

  describe('Review modal', () => {
    it('openReviewModal should show modal for unreviewed products', () => {
      component.openReviewModal(mockOrder);
      expect(component.showReviewModal()).toBe(true);
      expect(component.selectedProductId()).toBe('p1');
    });

    it('closeReviewModal should hide modal and reset state', () => {
      component.openReviewModal(mockOrder);
      component.closeReviewModal();
      expect(component.showReviewModal()).toBe(false);
      expect(component.reviewRating()).toBe(0);
    });

    it('submitReview should show error when no product selected', () => {
      component.orderToReview.set(mockOrder);
      component.selectedProductId.set('');
      component.submitReview();
      expect(addReview).not.toHaveBeenCalled();
    });

    it('submitReview should show error when no summary', () => {
      component.orderToReview.set(mockOrder);
      component.selectedProductId.set('p1');
      component.reviewSummary.set('');
      component.submitReview();
      expect(addReview).not.toHaveBeenCalled();
    });

    it('submitReview should show error when no rating', () => {
      component.orderToReview.set(mockOrder);
      component.selectedProductId.set('p1');
      component.reviewSummary.set('Great product');
      component.reviewRating.set(0);
      component.submitReview();
      expect(addReview).not.toHaveBeenCalled();
    });

    it('submitReview should call addReview on valid input', () => {
      addReview.mockReturnValue(of({ data: { reviewId: 'r1' } }));
      component.orderToReview.set(mockOrder);
      component.selectedProductId.set('p1');
      component.reviewSummary.set('Great product');
      component.reviewRating.set(5);
      component.submitReview();
      expect(addReview).toHaveBeenCalledWith('p1', 'Great product', 5);
    });
  });

  describe('Rating helpers', () => {
    it('setRating should update reviewRating', () => {
      component.setRating(4);
      expect(component.reviewRating()).toBe(4);
    });

    it('setHoverRating should update reviewHoverRating', () => {
      component.setHoverRating(3);
      expect(component.reviewHoverRating()).toBe(3);
    });

    it('getStarClass should return yellow for active stars', () => {
      component.reviewRating.set(3);
      expect(component.getStarClass(2)).toContain('yellow');
    });

    it('getStarClass should return gray for inactive stars', () => {
      component.reviewRating.set(2);
      expect(component.getStarClass(5)).toContain('gray');
    });
  });

  describe('getLineTotal', () => {
    it('should return price * quantity', () => {
      expect(component.getLineTotal(100, 3)).toBe(300);
    });
  });

  describe('hasReview / hasProductReview', () => {
    it('hasProductReview should return false initially', () => {
      expect(component.hasProductReview('p1')).toBe(false);
    });

    it('hasProductReview should return true after review added', () => {
      component.reviews.set([{ productId: 'p1', summary: 'Good', rating: 5 }]);
      expect(component.hasProductReview('p1')).toBe(true);
    });
  });

  describe('View helpers', () => {
    it('viewAddress should set selectedOrder and show address', () => {
      component.viewAddress(mockOrder);
      expect(component.selectedOrder()).toEqual(mockOrder);
      expect(component.showAddress()).toBe(true);
      expect(component.showProducts()).toBe(false);
    });

    it('viewProducts should set selectedOrder and show products', () => {
      component.viewProducts(mockOrder);
      expect(component.selectedOrder()).toEqual(mockOrder);
      expect(component.showProducts()).toBe(true);
      expect(component.showAddress()).toBe(false);
    });

    it('closeDetails should clear all detail views', () => {
      component.viewAddress(mockOrder);
      component.closeDetails();
      expect(component.showAddress()).toBe(false);
      expect(component.selectedOrder()).toBeNull();
    });
  });

  describe('getStatusColor', () => {
    it('should return yellow for not delivered', () => {
      expect(component.getStatusColor('not delivered')).toContain('yellow');
    });

    it('should return green for delivered', () => {
      expect(component.getStatusColor('delivered')).toContain('green');
    });

    it('should return red for cancelled', () => {
      expect(component.getStatusColor('cancelled')).toContain('red');
    });
  });

  describe('getProductIcon', () => {
    it('should return smartphone for phone products', () => {
      expect(component.getProductIcon('iPhone 15')).toBe('smartphone');
    });

    it('should return laptop for laptop products', () => {
      expect(component.getProductIcon('MacBook Pro')).toBe('laptop');
    });

    it('should return shopping_bag for unknown products', () => {
      expect(component.getProductIcon('Random Item')).toBe('shopping_bag');
    });
  });
});
