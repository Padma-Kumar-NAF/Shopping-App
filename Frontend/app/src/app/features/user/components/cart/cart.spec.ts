import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { Cart } from './cart';
import { CartService } from '../../../services/userServices/cart.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('Cart', () => {
  let component: Cart;
  let fixture: ComponentFixture<Cart>;

  const navigate = vi.fn();
  const GetUserCart = vi.fn();
  const removeAllFromCart = vi.fn();
  const updateCart = vi.fn();
  const removeFromCart = vi.fn();

  const mockCartItem: any = { cartItemId: 'ci1', productId: 'p1', productName: 'Laptop', price: 50000, quantity: 2, imagePath: '' };
  const mockCartItem2: any = { cartItemId: 'ci2', productId: 'p2', productName: 'Phone', price: 20000, quantity: 1, imagePath: '' };

  beforeEach(async () => {
    vi.clearAllMocks();
    GetUserCart.mockReturnValue(of({ data: { cartId: 'cart1', cartItems: [mockCartItem, mockCartItem2] } }));

    await TestBed.configureTestingModule({
      imports: [Cart],
      providers: [
        { provide: CartService, useValue: { GetUserCart, removeAllFromCart, updateCart, removeFromCart } },
        { provide: Router, useValue: { navigate } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(Cart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load cart items on init', () => {
    expect(component.cartItems().length).toBe(2);
    expect(component.isLoading()).toBe(false);
  });

  describe('Computed getters', () => {
    it('totalItems should return count of cart items', () => {
      expect(component.totalItems).toBe(2);
    });

    it('totalPrice should sum price * quantity', () => {
      expect(component.totalPrice).toBe(120000);
    });

    it('totalPages should compute based on itemsPerPage', () => {
      expect(component.totalPages).toBe(1);
    });

    it('paginatedItems should return items for current page', () => {
      expect(component.paginatedItems.length).toBe(2);
    });
  });

  describe('removeAllCart', () => {
    it('should clear cartItems on success', () => {
      removeAllFromCart.mockReturnValue(of({ message: 'Cleared' }));
      component.removeAllCart();
      expect(component.cartItems()).toEqual([]);
    });
  });

  describe('proceedToCheckout', () => {
    it('should navigate to /payment with fromCart query param', () => {
      component.proceedToCheckout();
      expect(navigate).toHaveBeenCalledWith(['/payment'], { queryParams: { fromCart: 'true' } });
    });
  });

  describe('increaseQty', () => {
    it('should call updateCart with incremented quantity', () => {
      updateCart.mockReturnValue(of({ data: { isUpdated: true } }));
      component.increaseQty(mockCartItem);
      expect(updateCart).toHaveBeenCalledWith(expect.objectContaining({ quantity: 3 }));
    });
  });

  describe('decreaseQty', () => {
    it('should call updateCart with decremented quantity', () => {
      updateCart.mockReturnValue(of({ data: { isUpdated: true } }));
      component.decreaseQty(mockCartItem);
      expect(updateCart).toHaveBeenCalledWith(expect.objectContaining({ quantity: 1 }));
    });

    it('should not call updateCart when quantity is 1', () => {
      component.decreaseQty(mockCartItem2);
      expect(updateCart).not.toHaveBeenCalled();
    });
  });

  describe('removeFromCart', () => {
    it('should remove item from cartItems on success', () => {
      removeFromCart.mockReturnValue(of({ message: 'Removed' }));
      component.removeFromCart(mockCartItem);
      expect(component.cartItems().find((i: any) => i.cartItemId === 'ci1')).toBeUndefined();
    });
  });

  describe('Pagination', () => {
    it('goToPage should set currentPage', () => {
      component.goToPage(2);
      expect(component.currentPage).toBe(2);
    });

    it('nextPage should increment currentPage when multiple pages exist', () => {
      const items = Array.from({ length: 7 }, (_, i) => ({ ...mockCartItem, cartItemId: `ci${i}` }));
      component.cartItems.set(items);
      component.currentPage = 1;
      component.nextPage();
      expect(component.currentPage).toBe(2);
    });

    it('prevPage should decrement currentPage', () => {
      component.currentPage = 2;
      component.prevPage();
      expect(component.currentPage).toBe(1);
    });

    it('prevPage should not go below 1', () => {
      component.currentPage = 1;
      component.prevPage();
      expect(component.currentPage).toBe(1);
    });
  });
});
