import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, BehaviorSubject } from 'rxjs';
import { vi } from 'vitest';
import { OrdersManagement } from './orders-management';
import { AdminOrderService } from '../../../services/adminServices/orders.service';
import { StoreService } from '../../../services/adminServices/store.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('OrdersManagement', () => {
  let component: OrdersManagement;
  let fixture: ComponentFixture<OrdersManagement>;

  const getAllOrders = vi.fn();
  const updateOrder = vi.fn();
  const refundOrder = vi.fn();
  const setOrders = vi.fn();
  const appendOrders = vi.fn();

  const mockOrders: any[] = [
    {
      orderId: 'o1',
      status: 'Not Delivered',
      totalAmount: 5000,
      isRefunded: false,
      orderBy: { userName: 'Alice', userEmail: 'alice@example.com' },
      payment: { paymentId: 'pay1' },
    },
    {
      orderId: 'o2',
      status: 'Delivered',
      totalAmount: 3000,
      isRefunded: false,
      orderBy: { userName: 'Bob', userEmail: 'bob@example.com' },
      payment: { paymentId: 'pay2' },
    },
  ];

  const storeState$ = new BehaviorSubject({ orders: mockOrders });

  beforeEach(async () => {
    vi.clearAllMocks();
    getAllOrders.mockReturnValue(of({ data: { items: mockOrders } }));

    await TestBed.configureTestingModule({
      imports: [OrdersManagement],
      providers: [
        { provide: AdminOrderService, useValue: { getAllOrders, updateOrder, refundOrder } },
        {
          provide: StoreService,
          useValue: {
            state$: storeState$.asObservable(),
            value: { orders: mockOrders },
            pageCache: { orders: new Set([1]) },
            setOrders, appendOrders,
          },
        },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(OrdersManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('pagedOrders', () => {
    it('should return all orders when no filter applied', () => {
      expect(component.pagedOrders.length).toBe(2);
    });

    it('should filter by status', () => {
      component.filterStatus.set('Delivered');
      expect(component.pagedOrders.length).toBe(1);
      expect(component.pagedOrders[0].status).toBe('Delivered');
    });

    it('should filter by search term', () => {
      component.searchTerm.set('alice');
      expect(component.pagedOrders.length).toBe(1);
    });
  });

  describe('updateSearch', () => {
    it('should update searchTerm and reset page', () => {
      component.currentPage.set(3);
      const event = { target: { value: 'bob' } } as any;
      component.updateSearch(event);
      expect(component.searchTerm()).toBe('bob');
      expect(component.currentPage()).toBe(1);
    });
  });

  describe('updateStatusFilter', () => {
    it('should update filterStatus and reset page', () => {
      const event = { target: { value: 'Shipped' } } as any;
      component.updateStatusFilter(event);
      expect(component.filterStatus()).toBe('Shipped');
    });
  });

  describe('toggleOrderDetails', () => {
    it('should expand order details', () => {
      component.toggleOrderDetails('o1');
      expect(component.expandedOrderId()).toBe('o1');
    });

    it('should collapse when same order is toggled again', () => {
      component.expandedOrderId.set('o1');
      component.toggleOrderDetails('o1');
      expect(component.expandedOrderId()).toBeNull();
    });
  });

  describe('Status modal', () => {
    it('openStatusModal should set pending order and show modal', () => {
      component.openStatusModal('o1', 'Not Delivered');
      expect(component.pendingStatusOrderId()).toBe('o1');
      expect(component.selectedStatus()).toBe('Not Delivered');
      expect(component.statusModal()).toBe(true);
    });

    it('cancelStatusChange should hide modal and clear state', () => {
      component.openStatusModal('o1', 'Not Delivered');
      component.cancelStatusChange();
      expect(component.statusModal()).toBe(false);
      expect(component.pendingStatusOrderId()).toBe('');
    });

    it('confirmStatusChange should call updateOrder and update store', () => {
      updateOrder.mockReturnValue(of({}));
      component.pendingStatusOrderId.set('o1');
      component.selectedStatus.set('Shipped');
      component.confirmStatusChange();
      expect(updateOrder).toHaveBeenCalledWith('o1', 'Shipped');
      expect(setOrders).toHaveBeenCalled();
    });

    it('setSelectedStatus should update selectedStatus', () => {
      component.setSelectedStatus('Delivered');
      expect(component.selectedStatus()).toBe('Delivered');
    });
  });

  describe('giveRefund', () => {
    it('should call refundOrder and update store on success', () => {
      refundOrder.mockReturnValue(of({ message: 'Refund Successful' }));
      component.giveRefund(mockOrders[0]);
      expect(refundOrder).toHaveBeenCalled();
      expect(setOrders).toHaveBeenCalled();
    });
  });

  describe('Pagination', () => {
    it('prevPage should decrement currentPage', () => {
      component.currentPage.set(2);
      component.prevPage();
      expect(component.currentPage()).toBe(1);
    });

    it('prevPage should not go below 1', () => {
      component.currentPage.set(1);
      component.prevPage();
      expect(component.currentPage()).toBe(1);
    });
  });

  describe('getStatusColor', () => {
    it('should return yellow for Not Delivered', () => {
      expect(component.getStatusColor('Not Delivered')).toContain('yellow');
    });

    it('should return green for Delivered', () => {
      expect(component.getStatusColor('Delivered')).toContain('green');
    });

    it('should return red for Cancelled', () => {
      expect(component.getStatusColor('Cancelled')).toContain('red');
    });

    it('should return gray for unknown status', () => {
      expect(component.getStatusColor('Unknown')).toContain('gray');
    });
  });
});
