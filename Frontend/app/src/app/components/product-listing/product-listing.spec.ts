import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError, Subject } from 'rxjs';
import { vi } from 'vitest';
import { ProductListing } from './product-listing';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { AuthStateService } from '../../services/auth-state.service';
import { RedirectService } from '../../services/redirect.service';
import { AdminCategoryService } from '../../services/adminServices/category.service';

describe('ProductListing', () => {
  let component: ProductListing;
  let fixture: ComponentFixture<ProductListing>;
  let queryParams$: Subject<any>;

  const navigate = vi.fn();
  const getProductsWithFilter = vi.fn();
  const searchProducts = vi.fn();
  const setSelectedProduct = vi.fn();
  const isAuthenticated = vi.fn().mockReturnValue(true);
  const storeIntendedRoute = vi.fn();
  const getAllCategories = vi.fn();

  const mockProducts: any[] = [
    { productId: '1', productName: 'Laptop', price: 50000, categoryId: 'c1', categoryName: 'Electronics', imagePath: '', review: [], quantity: 10 },
    { productId: '2', productName: 'Phone', price: 20000, categoryId: 'c1', categoryName: 'Electronics', imagePath: '', review: [{ reviewPoints: 4 }], quantity: 5 },
  ];
  const mockCategories: any[] = [{ categoryId: 'c1', categoryName: 'Electronics', productsCount: 2, createdAt: new Date() }];

  beforeEach(async () => {
    queryParams$ = new Subject();
    vi.clearAllMocks();
    getProductsWithFilter.mockReturnValue(of({ data: { productList: mockProducts }, action: '' }));
    searchProducts.mockReturnValue(of(mockProducts));
    getAllCategories.mockReturnValue(of({ data: { categoryList: mockCategories } }));
    isAuthenticated.mockReturnValue(true);

    await TestBed.configureTestingModule({
      imports: [ProductListing],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: ActivatedRoute, useValue: { queryParams: queryParams$.asObservable() } },
        { provide: ProductService, useValue: { getProductsWithFilter, searchProducts } },
        { provide: ProductStateService, useValue: { setSelectedProduct } },
        { provide: AuthStateService, useValue: { isAuthenticated } },
        { provide: RedirectService, useValue: { storeIntendedRoute } },
        { provide: AdminCategoryService, useValue: { getAllCategories } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductListing);
    component = fixture.componentInstance;
    fixture.detectChanges();
    queryParams$.next({});
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load categories on init', () => {
    expect(getAllCategories).toHaveBeenCalled();
    expect(component.categories()).toEqual(mockCategories);
  });

  describe('fetchFiltered', () => {
    it('should load products and set filteredProducts', () => {
      component.fetchFiltered();
      expect(getProductsWithFilter).toHaveBeenCalled();
      expect(component.filteredProducts()).toEqual(mockProducts);
      expect(component.isLoading()).toBe(false);
    });

    it('should set empty array when action is ShowEmptyPage', () => {
      getProductsWithFilter.mockReturnValue(of({ action: 'ShowEmptyPage', data: null }));
      component.fetchFiltered();
      expect(component.filteredProducts()).toEqual([]);
    });

    it('should set error on failure', () => {
      getProductsWithFilter.mockReturnValue(throwError(() => ({ error: { message: 'Error' } })));
      component.fetchFiltered();
      expect(component.error()).toBe('Error');
    });
  });

  describe('performSearch', () => {
    it('should call searchProducts and set filteredProducts', () => {
      component.performSearch('laptop');
      expect(searchProducts).toHaveBeenCalledWith('laptop');
    });

    it('should call fetchFiltered when query is empty', () => {
      const spy = vi.spyOn(component, 'fetchFiltered');
      component.performSearch('');
      expect(spy).toHaveBeenCalled();
    });

    it('should apply client-side price filter on search results', () => {
      component.priceRangeMin.set(30000);
      component.priceRangeMax.set(100000);
      component.performSearch('laptop');
      const results = component.filteredProducts();
      results.forEach(p => expect(p.price).toBeGreaterThanOrEqual(30000));
    });
  });

  describe('selectCategory', () => {
    it('should set selectedCategoryId and name, then fetch', () => {
      const spy = vi.spyOn(component, 'fetchFiltered');
      component.selectCategory('c1', 'Electronics');
      expect(component.selectedCategoryId()).toBe('c1');
      expect(component.selectedCategoryName()).toBe('Electronics');
      expect(spy).toHaveBeenCalled();
    });
  });

  describe('setPriceRange', () => {
    it('should update price range signals and fetch', () => {
      const spy = vi.spyOn(component, 'fetchFiltered');
      component.setPriceRange(1000, 5000);
      expect(component.priceRangeMin()).toBe(1000);
      expect(component.priceRangeMax()).toBe(5000);
      expect(spy).toHaveBeenCalled();
    });
  });

  describe('clearAllFilters', () => {
    it('should reset all filters', () => {
      component.selectedCategoryId.set('c1');
      component.priceRangeMin.set(5000);
      component.clearAllFilters();
      expect(component.selectedCategoryId()).toBeNull();
      expect(component.selectedCategoryName()).toBe('all');
      expect(component.priceRangeMin()).toBe(component.PRICE_MIN);
      expect(component.priceRangeMax()).toBe(component.PRICE_MAX);
    });
  });

  describe('onSearch', () => {
    it('should navigate with query and perform search', () => {
      const spy = vi.spyOn(component, 'performSearch');
      component.searchQuery.set('phone');
      component.onSearch();
      expect(component.hasSearched()).toBe(true);
      expect(spy).toHaveBeenCalledWith('phone');
    });

    it('should call clearSearch when query is empty', () => {
      const spy = vi.spyOn(component, 'clearSearch');
      component.searchQuery.set('');
      component.onSearch();
      expect(spy).toHaveBeenCalled();
    });
  });

  describe('clearSearch', () => {
    it('should reset search state', () => {
      component.searchQuery.set('laptop');
      component.hasSearched.set(true);
      component.clearSearch();
      expect(component.searchQuery()).toBe('');
      expect(component.hasSearched()).toBe(false);
    });
  });

  describe('toggleFilters', () => {
    it('should toggle showFilters', () => {
      expect(component.showFilters()).toBe(false);
      component.toggleFilters();
      expect(component.showFilters()).toBe(true);
    });
  });

  describe('getActiveFiltersCount', () => {
    it('should return 0 when no filters active', () => {
      expect(component.getActiveFiltersCount()).toBe(0);
    });

    it('should count category filter', () => {
      component.selectedCategoryId.set('c1');
      expect(component.getActiveFiltersCount()).toBe(1);
    });

    it('should count price filter', () => {
      component.priceRangeMin.set(1000);
      expect(component.getActiveFiltersCount()).toBe(1);
    });
  });

  describe('viewProductDetail', () => {
    it('should set selected product and navigate', () => {
      component.viewProductDetail(mockProducts[0]);
      expect(setSelectedProduct).toHaveBeenCalledWith(mockProducts[0]);
      expect(navigate).toHaveBeenCalledWith(['/product-detail', '1']);
    });
  });

  describe('buyNow', () => {
    it('should navigate to /payment when authenticated', () => {
      const event = { stopPropagation: vi.fn() } as any;
      component.buyNow(mockProducts[0], event);
      expect(event.stopPropagation).toHaveBeenCalled();
      expect(navigate).toHaveBeenCalledWith(['/payment'], expect.any(Object));
    });

    it('should redirect to /auth when not authenticated', () => {
      isAuthenticated.mockReturnValue(false);
      const event = { stopPropagation: vi.fn() } as any;
      component.buyNow(mockProducts[0], event);
      expect(navigate).toHaveBeenCalledWith(['/auth']);
    });
  });

  describe('getAverageRating', () => {
    it('should return null for product with no reviews', () => {
      expect(component.getAverageRating(mockProducts[0])).toBeNull();
    });

    it('should return average rating', () => {
      expect(component.getAverageRating(mockProducts[1])).toBe(4);
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
