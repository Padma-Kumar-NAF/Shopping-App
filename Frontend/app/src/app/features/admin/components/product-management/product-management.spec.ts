import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, BehaviorSubject } from 'rxjs';
import { vi } from 'vitest';
import { ProductManagement } from './product-management';
import { AdminProductService } from '../../../services/adminServices/products.service';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { StoreService } from '../../../services/adminServices/store.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('ProductManagement', () => {
  let component: ProductManagement;
  let fixture: ComponentFixture<ProductManagement>;

  const getAllProducts = vi.fn();
  const addProduct = vi.fn();
  const updateProduct = vi.fn();
  const deleteProduct = vi.fn();
  const getAllCategories = vi.fn();
  const appendProducts = vi.fn();
  const addProductStore = vi.fn();
  const updateProductStore = vi.fn();
  const setProducts = vi.fn();

  const mockProducts: any[] = [
    { productId: 'p1', name: 'Laptop', price: 50000, categoryId: 'c1', categoryName: 'Electronics', imagePath: '', description: 'A laptop', quantity: 10, review: [] },
    { productId: 'p2', name: 'Phone', price: 20000, categoryId: 'c2', categoryName: 'Mobile', imagePath: '', description: 'A phone', quantity: 5, review: [{ reviewPoints: 4 }] },
  ];
  const mockCategories: any[] = [
    { categoryId: 'c1', categoryName: 'Electronics' },
    { categoryId: 'c2', categoryName: 'Mobile' },
  ];

  const storeState$ = new BehaviorSubject({ products: mockProducts, categories: mockCategories });

  beforeEach(async () => {
    vi.clearAllMocks();
    getAllProducts.mockReturnValue(of({ data: { productList: mockProducts } }));
    getAllCategories.mockReturnValue(of({ data: { categoryList: mockCategories } }));

    await TestBed.configureTestingModule({
      imports: [ProductManagement],
      providers: [
        { provide: AdminProductService, useValue: { getAllProducts, addProduct, updateProduct, deleteProduct } },
        { provide: AdminCategoryService, useValue: { getAllCategories } },
        {
          provide: StoreService,
          useValue: {
            state$: storeState$.asObservable(),
            value: { products: mockProducts, categories: mockCategories },
            pageCache: { products: new Set([1]) },
            appendProducts, addProduct: addProductStore, updateProduct: updateProductStore, setProducts,
          },
        },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with list view', () => {
    expect(component.activeView()).toBe('list');
  });

  describe('switchView', () => {
    it('should switch to add view', () => {
      component.switchView('add');
      expect(component.activeView()).toBe('add');
    });

    it('should switch to bulk view', () => {
      component.switchView('bulk');
      expect(component.activeView()).toBe('bulk');
    });

    it('should switch back to list view', () => {
      component.switchView('add');
      component.switchView('list');
      expect(component.activeView()).toBe('list');
    });
  });

  describe('onSearchChange', () => {
    it('should update searchQuery', () => {
      const event = { target: { value: 'laptop' } } as any;
      component.onSearchChange(event);
      expect(component.searchQuery()).toBe('laptop');
    });
  });

  describe('onCategoryChange', () => {
    it('should update selectedCategoryId', () => {
      const event = { target: { value: 'c1' } } as any;
      component.onCategoryChange(event);
      expect(component.selectedCategoryId()).toBe('c1');
    });
  });

  describe('validateAddForm', () => {
    it('should return false when required fields are missing', () => {
      component.addForm.set({ categoryId: '', name: '', imagePath: '', description: '', price: 0, quantity: 0 });
      expect(component.validateAddForm()).toBe(false);
    });

    it('should return true when all required fields are filled', () => {
      component.addForm.set({ categoryId: 'c1', name: 'Test', imagePath: 'img.jpg', description: 'Desc', price: 100, quantity: 5 });
      expect(component.validateAddForm()).toBe(true);
    });
  });

  describe('addProduct', () => {
    it('should not call API when form is invalid', () => {
      component.addForm.set({ categoryId: '', name: '', imagePath: '', description: '', price: 0, quantity: 0 });
      component.addProduct();
      expect(addProduct).not.toHaveBeenCalled();
    });

    it('should call addProduct API on valid form', () => {
      addProduct.mockReturnValue(of({ data: { productId: 'p3' } }));
      component.addForm.set({ categoryId: 'c1', name: 'New Product', imagePath: 'img.jpg', description: 'Desc', price: 100, quantity: 5 });
      component.addProduct();
      expect(addProduct).toHaveBeenCalled();
    });
  });

  describe('Edit modal', () => {
    it('openEditModal should set editingProduct and show modal', () => {
      component.openEditModal(mockProducts[0]);
      expect(component.editingProduct()).toEqual(mockProducts[0]);
      expect(component.showEditModal()).toBe(true);
      expect(component.editForm().name).toBe('Laptop');
    });

    it('closeEditModal should hide modal and clear editingProduct', () => {
      component.openEditModal(mockProducts[0]);
      component.closeEditModal();
      expect(component.showEditModal()).toBe(false);
      expect(component.editingProduct()).toBeNull();
    });

    it('saveProduct should call updateProduct API', () => {
      updateProduct.mockReturnValue(of({ data: { isSuccess: true } }));
      component.openEditModal(mockProducts[0]);
      component.saveProduct();
      expect(updateProduct).toHaveBeenCalled();
    });
  });

  describe('Delete modal', () => {
    it('openDeleteModal should set pendingDeleteId and show modal', () => {
      component.openDeleteModal('p1');
      expect(component.pendingDeleteId()).toBe('p1');
      expect(component.confirmModal()).toBe(true);
    });

    it('cancelDelete should clear id and hide modal', () => {
      component.openDeleteModal('p1');
      component.cancelDelete();
      expect(component.pendingDeleteId()).toBe('');
      expect(component.confirmModal()).toBe(false);
    });

    it('deleteProduct should call deleteProduct API and update store', () => {
      deleteProduct.mockReturnValue(of({ data: { isSuccess: true } }));
      component.pendingDeleteId.set('p1');
      component.deleteProduct();
      expect(deleteProduct).toHaveBeenCalledWith('p1');
      expect(setProducts).toHaveBeenCalled();
    });
  });

  describe('resetAddForm', () => {
    it('should reset addForm to empty values', () => {
      component.addForm.set({ categoryId: 'c1', name: 'Test', imagePath: 'img.jpg', description: 'Desc', price: 100, quantity: 5 });
      component.resetAddForm();
      expect(component.addForm().name).toBe('');
      expect(component.addForm().price).toBe(0);
    });
  });

  describe('avgRating', () => {
    it('should return 0 for empty reviews', () => {
      expect(component.avgRating([])).toBe('0');
    });

    it('should return average rating', () => {
      const reviews: any[] = [{ reviewPoints: 4 }, { reviewPoints: 5 }];
      expect(component.avgRating(reviews)).toBe('4.5');
    });
  });
});
