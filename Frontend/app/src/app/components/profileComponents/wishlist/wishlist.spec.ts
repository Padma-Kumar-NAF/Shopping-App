import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { WishlistComponent } from './wishlist';
import { WishlistService } from '../../../services/wishlist.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('WishlistComponent', () => {
  let component: WishlistComponent;
  let fixture: ComponentFixture<WishlistComponent>;

  const getUserWishlists = vi.fn();
  const createWishlist = vi.fn();
  const deleteWishlist = vi.fn();
  const removeProduct = vi.fn();

  const mockWishlists: any[] = [
    { wishListId: 'wl1', wishListName: 'Favourites', wishListItems: [{ productId: 'p1', productName: 'Laptop' }] },
    { wishListId: 'wl2', wishListName: 'Later', wishListItems: [] },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    getUserWishlists.mockReturnValue(of({ data: { wishList: mockWishlists } }));

    await TestBed.configureTestingModule({
      imports: [WishlistComponent],
      providers: [{ provide: WishlistService, useValue: { getUserWishlists, createWishlist, deleteWishlist, removeProduct } }],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(WishlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load wishlists on init', () => {
    expect(component.wishLists()).toEqual(mockWishlists);
    expect(component.selectedWishlistId()).toBe('wl1');
    expect(component.isLoading()).toBe(false);
  });

  it('isEmpty should be false when wishlists exist', () => {
    expect(component.isEmpty()).toBe(false);
  });

  it('isEmpty should be true when no wishlists', () => {
    component.wishLists.set([]);
    expect(component.isEmpty()).toBe(true);
  });

  it('selectedWishlist should return the selected wishlist', () => {
    component.selectedWishlistId.set('wl2');
    expect(component.selectedWishlist()?.wishListName).toBe('Later');
  });

  describe('selectWishlist', () => {
    it('should update selectedWishlistId', () => {
      component.selectWishlist('wl2');
      expect(component.selectedWishlistId()).toBe('wl2');
    });
  });

  describe('createWishlist', () => {
    it('should show error when name is empty', () => {
      component.newWishlistName.set('');
      component.createWishlist();
      expect(createWishlist).not.toHaveBeenCalled();
    });

    it('should call createWishlist API and reload on success', () => {
      createWishlist.mockReturnValue(of({ data: { isCreated: true } }));
      component.newWishlistName.set('New List');
      component.createWishlist();
      expect(createWishlist).toHaveBeenCalledWith('New List');
      expect(getUserWishlists).toHaveBeenCalledTimes(2);
    });
  });

  describe('removeWishlist', () => {
    it('should remove wishlist from list on success', () => {
      deleteWishlist.mockReturnValue(of({ data: { isDeleted: true } }));
      component.removeWishlist('wl2');
      expect(component.wishLists().find((w: any) => w.wishListId === 'wl2')).toBeUndefined();
    });

    it('should update selectedWishlistId when selected wishlist is removed', () => {
      deleteWishlist.mockReturnValue(of({ data: { isDeleted: true } }));
      component.selectedWishlistId.set('wl1');
      component.removeWishlist('wl1');
      expect(component.selectedWishlistId()).toBe('wl2');
    });
  });

  describe('removeProduct', () => {
    it('should remove product from wishlist on success', () => {
      removeProduct.mockReturnValue(of({ data: { isRemoved: true } }));
      component.removeProduct('wl1', 'p1');
      const wl = component.wishLists().find((w: any) => w.wishListId === 'wl1');
      expect(wl?.wishListItems.find((p: any) => p.productId === 'p1')).toBeUndefined();
    });
  });
});
