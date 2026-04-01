import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { HomeComponent } from './home';
import { ProductService } from '../../../services/product.service';
import { ProductStateService } from '../../../services/product-state.service';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  const navigate = vi.fn();
  const getAllProducts = vi.fn();
  const setSelectedProduct = vi.fn();
  const getAllCategories = vi.fn();

  const mockProducts: any[] = [
    { productId: '1', productName: 'Laptop', price: 50000, categoryId: 'c1', categoryName: 'Electronics', imagePath: '', review: [], quantity: 10 },
    { productId: '2', productName: 'Phone', price: 20000, categoryId: 'c1', categoryName: 'Electronics', imagePath: '', review: [{ reviewPoints: 4 }, { reviewPoints: 5 }], quantity: 5 },
  ];
  const mockCategories: any[] = [{ categoryId: 'c1', categoryName: 'Electronics', productsCount: 2, createdAt: new Date() }];

  beforeEach(async () => {
    vi.clearAllMocks();
    getAllProducts.mockReturnValue(of(mockProducts));
    getAllCategories.mockReturnValue(of({ data: { categoryList: mockCategories } }));

    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: ProductService, useValue: { getAllProducts } },
        { provide: ProductStateService, useValue: { setSelectedProduct } },
        { provide: AdminCategoryService, useValue: { getAllCategories } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load products and categories on init', () => {
    expect(component.products()).toEqual(mockProducts);
    expect(component.categories()).toEqual(mockCategories);
    expect(component.isLoading()).toBe(false);
  });

  it('should set error on loadData failure', () => {
    getAllProducts.mockReturnValue(throwError(() => ({ error: { message: 'Network error' } })));
    component.loadData();
    expect(component.error()).toBe('Network error');
    expect(component.isLoading()).toBe(false);
  });

  it('should have 6 carousel slides', () => {
    expect(component.carouselSlides.length).toBe(6);
  });

  describe('onProductClick', () => {
    it('should set selected product and navigate to product-detail', () => {
      component.onProductClick(mockProducts[0]);
      expect(setSelectedProduct).toHaveBeenCalledWith(mockProducts[0]);
      expect(navigate).toHaveBeenCalledWith(['/product-detail', '1']);
    });
  });

  describe('onCategoryClick', () => {
    it('should navigate to /products with category query param', () => {
      component.onCategoryClick(mockCategories[0]);
      expect(navigate).toHaveBeenCalledWith(['/products'], { queryParams: { category: 'Electronics' } });
    });
  });

  describe('onCategorySelect', () => {
    it('should navigate to /products with category when value is set', () => {
      const event = { target: { value: 'Electronics' } } as any;
      component.onCategorySelect(event);
      expect(navigate).toHaveBeenCalledWith(['/products'], { queryParams: { category: 'Electronics' } });
    });

    it('should navigate to /products without query when value is empty', () => {
      const event = { target: { value: '' } } as any;
      component.onCategorySelect(event);
      expect(navigate).toHaveBeenCalledWith(['/products']);
    });
  });

  describe('onSearch', () => {
    it('should navigate to /products with search query', () => {
      component.searchQuery.set('laptop');
      component.onSearch();
      expect(navigate).toHaveBeenCalledWith(['/products'], { queryParams: { q: 'laptop' } });
    });

    it('should not navigate when query is empty', () => {
      component.searchQuery.set('');
      component.onSearch();
      expect(navigate).not.toHaveBeenCalled();
    });
  });

  describe('getAverageRating', () => {
    it('should return null for product with no reviews', () => {
      expect(component.getAverageRating(mockProducts[0])).toBeNull();
    });

    it('should return average rating for product with reviews', () => {
      expect(component.getAverageRating(mockProducts[1])).toBe(4.5);
    });
  });
});
