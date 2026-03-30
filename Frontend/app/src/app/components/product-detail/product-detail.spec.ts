import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { vi } from 'vitest';
import { ProductDetail } from './product-detail';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { AuthStateService } from '../../services/auth-state.service';
import { RedirectService } from '../../services/redirect.service';
import { CartService } from '../../services/cart.service';
import { WishlistService } from '../../services/wishlist.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('ProductDetail', () => {
  let component: ProductDetail;
  let fixture: ComponentFixture<ProductDetail>;
  let paramMap$: Subject<any>;

  const navigate = vi.fn();
  const getProductById = vi.fn();
  const getProductsByCategory = vi.fn().mockReturnValue(of([]));
  const getSelectedProduct = vi.fn();
  const setSelectedProduct = vi.fn();
  const isAuthenticated = vi.fn().mockReturnValue(true);
  const storeIntendedRoute = vi.fn();
  const addToCart = vi.fn();
  const getUserWishlists = vi.fn();
  const addProduct = vi.fn();

  const mockProduct: any = {
    productId: 'p1',
    productName: 'Laptop',
    price: 50000,
    categoryId: 'c1',
    categoryName: 'Electronics',
    imagePath: 'img.jpg',
    review: [{ reviewPoints: 4 }, { reviewPoints: 5 }],
    quantity: 10,
  };

  beforeEach(async () => {
    paramMap$ = new Subject();
    vi.clearAllMocks();
    getSelectedProduct.mockReturnValue(mockProduct);
    getProductsByCategory.mockReturnValue(of([]));
    isAuthenticated.mockReturnValue(true);

    await TestBed.configureTestingModule({
      imports: [ProductDetail],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: ActivatedRoute, useValue: { paramMap: paramMap$.asObservable() } },
        { provide: ProductService, useValue: { getProductById, getProductsByCategory } },
        { provide: ProductStateService, useValue: { getSelectedProduct, setSelectedProduct } },
        { provide: AuthStateService, useValue: { isAuthenticated } },
        { provide: RedirectService, useValue: { storeIntendedRoute } },
        { provide: CartService, useValue: { addToCart } },
        { provide: WishlistService, useValue: { getUserWishlists, addProduct } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
    paramMap$.next({ get: () => 'p1' });
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load product from state when productId matches', () => {
    expect(component.product()).toEqual(mockProduct);
    expect(component.selectedImage()).toBe('img.jpg');
  });

  it('should fetch product from API when not in state', () => {
    getSelectedProduct.mockReturnValue(null);
    getProductById.mockReturnValue(of(mockProduct));
    paramMap$.next({ get: () => 'p2' });
    expect(getProductById).toHaveBeenCalledWith('p2');
  });

  it('should set error when productId is null', () => {
    getSelectedProduct.mockReturnValue(null);
    paramMap$.next({ get: () => null });
    expect(component.error()).toBe('No product specified');
  });

  describe('increaseQuantity', () => {
    it('should increment quantity', () => {
      component.quantity.set(1);
      component.increaseQuantity();
      expect(component.quantity()).toBe(2);
    });

    it('should not exceed max stock', () => {
      component.quantity.set(10);
      component.increaseQuantity();
      expect(component.quantity()).toBe(10);
    });
  });

  describe('decreaseQuantity', () => {
    it('should decrement quantity', () => {
      component.quantity.set(3);
      component.decreaseQuantity();
      expect(component.quantity()).toBe(2);
    });

    it('should not go below 1', () => {
      component.quantity.set(1);
      component.decreaseQuantity();
      expect(component.quantity()).toBe(1);
    });
  });

  describe('addToCart', () => {
    it('should call cartService.addToCart when authenticated', () => {
      addToCart.mockReturnValue(of({ message: 'Added' }));
      component.addToCart();
      expect(addToCart).toHaveBeenCalled();
    });

    it('should redirect to /auth when not authenticated', () => {
      isAuthenticated.mockReturnValue(false);
      component.addToCart();
      expect(navigate).toHaveBeenCalledWith(['/auth']);
    });
  });

  describe('buyNow', () => {
    it('should navigate to /payment when authenticated', () => {
      component.buyNow();
      expect(navigate).toHaveBeenCalledWith(['/payment'], expect.any(Object));
    });

    it('should redirect to /auth when not authenticated', () => {
      isAuthenticated.mockReturnValue(false);
      component.buyNow();
      expect(navigate).toHaveBeenCalledWith(['/auth']);
    });
  });

  describe('Wishlist', () => {
    it('openWishlistPopup should load wishlists and show popup', () => {
      getUserWishlists.mockReturnValue(of({ data: { wishList: [] } }));
      component.openWishlistPopup();
      expect(component.showWishlistPopup()).toBe(true);
    });

    it('openWishlistPopup should redirect to /auth when not authenticated', () => {
      isAuthenticated.mockReturnValue(false);
      component.openWishlistPopup();
      expect(navigate).toHaveBeenCalledWith(['/auth']);
    });

    it('addToWishlist should call wishlistService.addProduct', () => {
      addProduct.mockReturnValue(of({ data: { isSuccess: true }, message: 'Added' }));
      component.addToWishlist('wl1');
      expect(addProduct).toHaveBeenCalledWith('wl1', 'p1');
      expect(component.showWishlistPopup()).toBe(false);
    });

    it('closeWishlistPopup should hide popup', () => {
      component.showWishlistPopup.set(true);
      component.closeWishlistPopup();
      expect(component.showWishlistPopup()).toBe(false);
    });
  });

  describe('viewRelatedProduct', () => {
    it('should set product state and navigate', () => {
      component.viewRelatedProduct(mockProduct);
      expect(setSelectedProduct).toHaveBeenCalledWith(mockProduct);
      expect(navigate).toHaveBeenCalledWith(['/product-detail', 'p1']);
    });
  });

  describe('getAverageRating', () => {
    it('should return average of review points', () => {
      expect(component.getAverageRating(mockProduct)).toBe(4.5);
    });

    it('should return null for empty reviews', () => {
      expect(component.getAverageRating({ ...mockProduct, review: [] })).toBeNull();
    });
  });

  describe('getStarArray', () => {
    it('should return array of 5 with filled stars', () => {
      const stars = component.getStarArray(3);
      expect(stars.length).toBe(5);
      expect(stars.filter(s => s === 1).length).toBe(3);
    });
  });

  describe('ngOnDestroy', () => {
    it('should complete destroy$ on destroy', () => {
      const spy = vi.spyOn((component as any).destroy$, 'next');
      component.ngOnDestroy();
      expect(spy).toHaveBeenCalled();
    });
  });
});
