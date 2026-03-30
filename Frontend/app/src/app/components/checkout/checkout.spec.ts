import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { CheckoutComponent } from './checkout';
import { CartService } from '../../services/cart.service';
import { AddressSelectionService } from '../../services/address-selection.service';
import { AddressApiService } from '../../services/userServices/address.service';
import { AuthStateService } from '../../services/auth-state.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('CheckoutComponent', () => {
  let component: CheckoutComponent;
  let fixture: ComponentFixture<CheckoutComponent>;

  const navigate = vi.fn();
  const GetUserCart = vi.fn();
  const orderAllFromCart = vi.fn();
  const getAvailableAddresses = vi.fn().mockReturnValue([]);
  const getSelectedAddress = vi.fn().mockReturnValue(null);
  const setAvailableAddresses = vi.fn();
  const setSelectedAddress = vi.fn();
  const GetUserAddresses = vi.fn();

  const mockAddress: any = { addressId: 'a1', addressLine1: '123 Main St', city: 'Chennai', state: 'TN', pincode: '600001' };
  const mockCartItem: any = { cartItemId: 'ci1', productId: 'p1', productName: 'Laptop', price: 50000, quantity: 2 };

  beforeEach(async () => {
    vi.clearAllMocks();
    GetUserCart.mockReturnValue(of({ data: { cartId: 'cart1', cartItems: [mockCartItem] } }));
    getAvailableAddresses.mockReturnValue([]);
    getSelectedAddress.mockReturnValue(null);
    GetUserAddresses.mockReturnValue(of({ data: { addressList: [mockAddress] } }));

    await TestBed.configureTestingModule({
      imports: [CheckoutComponent],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: CartService, useValue: { GetUserCart, orderAllFromCart } },
        { provide: AddressSelectionService, useValue: { getAvailableAddresses, getSelectedAddress, setAvailableAddresses, setSelectedAddress } },
        { provide: AddressApiService, useValue: { GetUserAddresses } },
        { provide: AuthStateService, useValue: {} },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(CheckoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load cart and addresses on init', () => {
    expect(GetUserCart).toHaveBeenCalled();
    expect(component.cartItems()).toEqual([mockCartItem]);
    expect(component.cartId()).toBe('cart1');
  });

  it('should load addresses from API when cache is empty', () => {
    expect(GetUserAddresses).toHaveBeenCalled();
    expect(component.availableAddresses()).toEqual([mockAddress]);
  });

  describe('Computed getters', () => {
    beforeEach(() => {
      component.cartItems.set([mockCartItem]);
    });

    it('subtotal should sum price * quantity', () => {
      expect(component.subtotal).toBe(100000);
    });

    it('tax should be 18% of subtotal', () => {
      expect(component.tax).toBeCloseTo(18000);
    });

    it('shipping should be 0 for orders above 5000', () => {
      expect(component.shipping).toBe(0);
    });

    it('shipping should be 99 for orders below 5000', () => {
      component.cartItems.set([{ ...mockCartItem, price: 100, quantity: 1 }]);
      expect(component.shipping).toBe(99);
    });

    it('total should be subtotal + tax + shipping', () => {
      expect(component.total).toBe(component.subtotal + component.tax + component.shipping);
    });
  });

  describe('selectAddress', () => {
    it('should set selectedAddress and close picker', () => {
      component.showAddressPicker.set(true);
      component.selectAddress(mockAddress);
      expect(component.selectedAddress()).toEqual(mockAddress);
      expect(component.showAddressPicker()).toBe(false);
      expect(setSelectedAddress).toHaveBeenCalledWith(mockAddress);
    });
  });

  describe('selectPaymentMethod', () => {
    it('should update selectedPaymentMethod', () => {
      component.selectPaymentMethod('upi');
      expect(component.selectedPaymentMethod()).toBe('upi');
    });
  });

  describe('processPayment', () => {
    it('should show error when no address selected', () => {
      component.selectedAddress.set(null);
      component.processPayment();
      expect(orderAllFromCart).not.toHaveBeenCalled();
    });

    it('should show error when card details are missing', () => {
      component.selectedAddress.set(mockAddress);
      component.selectedPaymentMethod.set('card');
      component.cardNumber.set('');
      component.processPayment();
      expect(orderAllFromCart).not.toHaveBeenCalled();
    });

    it('should place order and navigate to /profile/orders on success', () => {
      component.selectedAddress.set(mockAddress);
      component.selectedPaymentMethod.set('card');
      component.cardNumber.set('4111111111111111');
      component.cardName.set('Test User');
      component.expiryDate.set('12/26');
      component.cvv.set('123');
      component.cartId.set('cart1');
      orderAllFromCart.mockReturnValue(of({ data: { isSuccess: true }, message: 'Success' }));
      component.processPayment();
      expect(orderAllFromCart).toHaveBeenCalled();
      expect(navigate).toHaveBeenCalledWith(['/profile/orders']);
    });
  });
});
