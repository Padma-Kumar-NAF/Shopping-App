import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { vi, expect as vitestExpect } from 'vitest';
import { PaymentComponent } from './payment';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { CartService } from '../../services/cart.service';
import { AddressSelectionService } from '../../services/address-selection.service';
import { AddressApiService } from '../../services/userServices/address.service';
import { AuthStateService } from '../../services/auth-state.service';
import { OrderService } from '../../services/userServices/order.service';
import { PromoCodeService } from '../../services/adminServices/promocode.service';
import { WalletService } from '../../services/userServices/wallet.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('PaymentComponent', () => {
  let component: PaymentComponent;
  let fixture: ComponentFixture<PaymentComponent>;

  const navigate = vi.fn();
  const getProductById = vi.fn();
  const getSelectedProduct = vi.fn();
  const GetUserCart = vi.fn();
  const orderAllFromCart = vi.fn();
  const getAvailableAddresses = vi.fn().mockReturnValue([]);
  const getSelectedAddress = vi.fn().mockReturnValue(null);
  const setAvailableAddresses = vi.fn();
  const setSelectedAddress = vi.fn();
  const clearSelectedAddress = vi.fn();
  const GetUserAddresses = vi.fn();
  const username = vi.fn().mockReturnValue('Test User');
  const email = vi.fn().mockReturnValue('test@example.com');
  const placeOrder = vi.fn();
  const validatePromoCode = vi.fn();
  const getWalletBalance = vi.fn();

  const mockProduct: any = { productId: 'p1', productName: 'Laptop', price: 50000, quantity: 10 };
  const mockAddress: any = { addressId: 'a1', addressLine1: '123 Main St', addressLine2: '', city: 'Chennai', state: 'TN', pincode: '600001' };
  const mockAvailableAddresses$ = of([mockAddress]);

  beforeEach(async () => {
    vi.clearAllMocks();
    getSelectedProduct.mockReturnValue(mockProduct);
    getAvailableAddresses.mockReturnValue([]);
    getSelectedAddress.mockReturnValue(null);
    username.mockReturnValue('Test User');
    email.mockReturnValue('test@example.com');

    await TestBed.configureTestingModule({
      imports: [PaymentComponent],
      providers: [
        { provide: Router, useValue: { navigate } },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: {
                get: (key: string) => key === 'fromProduct' ? 'true' : key === 'quantity' ? '2' : null,
              },
            },
          },
        },
        { provide: ProductService, useValue: { getProductById } },
        { provide: ProductStateService, useValue: { getSelectedProduct, setSelectedProduct: vi.fn() } },
        { provide: CartService, useValue: { GetUserCart, orderAllFromCart } },
        {
          provide: AddressSelectionService,
          useValue: { getAvailableAddresses, getSelectedAddress, setAvailableAddresses, setSelectedAddress, clearSelectedAddress, availableAddresses$: mockAvailableAddresses$ },
        },
        { provide: AddressApiService, useValue: { GetUserAddresses } },
        { provide: AuthStateService, useValue: { username, email } },
        { provide: OrderService, useValue: { placeOrder } },
        { provide: PromoCodeService, useValue: { validatePromoCode } },
        { provide: WalletService, useValue: { getWalletBalance } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set paymentMode to single and load product from state', () => {
    expect(component.paymentMode()).toBe('single');
    expect(component.product()).toEqual(mockProduct);
    expect(component.quantity()).toBe(2);
  });

  describe('Computed getters', () => {
    beforeEach(() => {
      component.product.set(mockProduct);
      component.quantity.set(2);
      component.discountPercent.set(0);
    });

    it('subtotal should be price * quantity for single mode', () => {
      expect(component.subtotal).toBe(100000);
    });

    it('totalItems should return quantity for single mode', () => {
      expect(component.totalItems).toBe(2);
    });

    it('tax should be 18% of subtotal', () => {
      expect(component.tax).toBeCloseTo(18000);
    });

    it('shipping should be 0 for orders above 5000', () => {
      expect(component.shipping).toBe(0);
    });

    it('discountAmount should be 0 when no promo applied', () => {
      expect(component.discountAmount).toBe(0);
    });

    it('discountAmount should apply percentage', () => {
      component.discountPercent.set(10);
      expect(component.discountAmount).toBe(10000);
    });

    it('total should sum subtotal - discount + tax + shipping', () => {
      expect(component.total).toBe(component.subtotal - component.discountAmount + component.tax + component.shipping);
    });
  });

  describe('resolvedPaymentType', () => {
    it('should return stripe when stripe method selected', () => {
      component.selectedPaymentMethod.set('stripe');
      expect(component.resolvedPaymentType).toBe('stripe');
    });

    it('should return wallet when wallet covers all', () => {
      component.selectedPaymentMethod.set('wallet');
      component.walletBalance.set(999999);
      expect(component.resolvedPaymentType).toBe('wallet');
    });

    it('should return wallet+stripe when wallet is partial', () => {
      component.selectedPaymentMethod.set('wallet');
      component.walletBalance.set(100);
      expect(component.resolvedPaymentType).toBe('wallet+stripe');
    });
  });

  describe('selectAddress', () => {
    it('should apply address and close picker', () => {
      component.showAddressPicker.set(true);
      component.selectAddress(mockAddress);
      expect(component.selectedAddress()).toEqual(mockAddress);
      expect(component.showAddressPicker()).toBe(false);
      expect(component.city()).toBe('Chennai');
    });
  });

  describe('unselectAddress', () => {
    it('should clear selected address and fields', () => {
      component.selectAddress(mockAddress);
      component.unselectAddress();
      expect(component.selectedAddress()).toBeNull();
      expect(component.city()).toBe('');
    });
  });

  describe('applyPromo', () => {
    it('should show error when promo input is empty', () => {
      component.promoInput.set('');
      component.applyPromo();
      expect(validatePromoCode).not.toHaveBeenCalled();
      expect(component.promoError()).toBeTruthy();
    });

    it('should call validatePromoCode and apply discount on success', () => {
      validatePromoCode.mockReturnValue(of({ data: { isValid: true, discountPercentage: 10 } }));
      component.promoInput.set('SAVE10');
      component.applyPromo();
      expect(validatePromoCode).toHaveBeenCalled();
      expect(component.appliedPromo()).toBe('SAVE10');
      expect(component.discountPercent()).toBe(10);
    });

    it('should set promoError on invalid promo', () => {
      validatePromoCode.mockReturnValue(of({ data: { isValid: false }, message: 'Invalid code' }));
      component.promoInput.set('INVALID');
      component.applyPromo();
      expect(component.promoError()).toBe('Invalid code');
    });

    it('should not apply promo if already applied', () => {
      component.appliedPromo.set('EXISTING');
      component.applyPromo();
      expect(validatePromoCode).not.toHaveBeenCalled();
    });
  });

  describe('removePromo', () => {
    it('should clear promo state', () => {
      component.appliedPromo.set('SAVE10');
      component.discountPercent.set(10);
      component.removePromo();
      expect(component.appliedPromo()).toBeNull();
      expect(component.discountPercent()).toBe(0);
      expect(component.promoInput()).toBe('');
    });
  });

  describe('selectMethod', () => {
    it('should set selectedPaymentMethod to wallet and fetch balance', () => {
      getWalletBalance.mockReturnValue(of({ data: { walletBalance: 5000 } }));
      component.selectMethod('wallet');
      expect(component.selectedPaymentMethod()).toBe('wallet');
      expect(getWalletBalance).toHaveBeenCalled();
    });

    it('should clear wallet balance when switching to stripe', () => {
      component.walletBalance.set(5000);
      component.selectMethod('stripe');
      expect(component.walletBalance()).toBeNull();
    });
  });

  describe('updateQuantity', () => {
    it('should increment quantity', () => {
      component.quantity.set(1);
      component.updateQuantity(1);
      expect(component.quantity()).toBe(2);
    });

    it('should decrement quantity', () => {
      component.quantity.set(3);
      component.updateQuantity(-1);
      expect(component.quantity()).toBe(2);
    });

    it('should not go below 1', () => {
      component.quantity.set(1);
      component.updateQuantity(-1);
      expect(component.quantity()).toBe(1);
    });

    it('should not exceed product stock', () => {
      component.quantity.set(10);
      component.updateQuantity(1);
      expect(component.quantity()).toBe(10);
    });
  });

  describe('processPayment', () => {
    it('should show error when shipping details are incomplete', () => {
      component.fullName.set('');
      component.processPayment();
      expect(placeOrder).not.toHaveBeenCalled();
    });

    it('should show error when no address selected', () => {
      component.fullName.set('Test');
      component.email.set('test@example.com');
      component.phone.set('9876543210');
      component.address.set('123 Main St');
      component.city.set('Chennai');
      component.state.set('TN');
      component.pincode.set('600001');
      component.selectedAddress.set(null);
      component.processPayment();
      expect(placeOrder).not.toHaveBeenCalled();
    });
  });

  describe('closeStripeModal', () => {
    it('should hide stripe modal and set isProcessing to false', () => {
      component.showStripeModal.set(true);
      component.isProcessing.set(true);
      component.closeStripeModal();
      expect(component.showStripeModal()).toBe(false);
      expect(component.isProcessing()).toBe(false);
    });
  });
});
