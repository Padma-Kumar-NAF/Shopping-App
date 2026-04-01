import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, BehaviorSubject } from 'rxjs';
import { vi } from 'vitest';
import { CategoryManagement } from './category-management';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { StoreService } from '../../../services/adminServices/store.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('CategoryManagement', () => {
  let component: CategoryManagement;
  let fixture: ComponentFixture<CategoryManagement>;

  const getAllCategories = vi.fn();
  const addCategory = vi.fn();
  const updateCategory = vi.fn();
  const deleteCategory = vi.fn();
  const appendCategories = vi.fn();
  const addCategoryStore = vi.fn();
  const updateCategoryStore = vi.fn();
  const setCategories = vi.fn();

  const mockCategories: any[] = [
    { categoryId: 'c1', categoryName: 'Electronics', productsCount: 5, createdAt: new Date() },
    { categoryId: 'c2', categoryName: 'Clothing', productsCount: 3, createdAt: new Date() },
  ];

  const storeState$ = new BehaviorSubject({ categories: mockCategories });

  beforeEach(async () => {
    vi.clearAllMocks();
    getAllCategories.mockReturnValue(of({ data: { categoryList: mockCategories } }));

    await TestBed.configureTestingModule({
      imports: [CategoryManagement],
      providers: [
        { provide: AdminCategoryService, useValue: { getAllCategories, addCategory, updateCategory, deleteCategory } },
        {
          provide: StoreService,
          useValue: {
            state$: storeState$.asObservable(),
            value: { categories: mockCategories },
            pageCache: { categories: new Set([1]) },
            appendCategories,
            addCategory: addCategoryStore,
            updateCategory: updateCategoryStore,
            setCategories,
          },
        },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Computed getters', () => {
    it('pagedCategories should return slice of categories', () => {
      component.currentPage.set(1);
      expect(component.pagedCategories.length).toBeLessThanOrEqual(component.pageSize);
    });

    it('totalCategories should return store categories count', () => {
      expect(component.totalCategories).toBe(2);
    });
  });

  describe('toggleAddForm', () => {
    it('should toggle showAddForm', () => {
      expect(component.showAddForm()).toBe(false);
      component.toggleAddForm();
      expect(component.showAddForm()).toBe(true);
    });

    it('should reset form when closing', () => {
      component.newCategory.set('Test');
      component.showAddForm.set(true);
      component.toggleAddForm();
      expect(component.newCategory()).toBe('');
    });
  });

  describe('addCategory', () => {
    it('should show error when name is empty', () => {
      component.newCategory.set('');
      component.addCategory();
      expect(addCategory).not.toHaveBeenCalled();
    });

    it('should show error when category already exists', () => {
      component.newCategory.set('Electronics');
      component.addCategory();
      expect(addCategory).not.toHaveBeenCalled();
    });

    it('should call addCategory API and update store on success', () => {
      addCategory.mockReturnValue(of({ data: { categoryId: 'c3' } }));
      component.newCategory.set('Books');
      component.addCategory();
      expect(addCategory).toHaveBeenCalledWith('Books');
      expect(addCategoryStore).toHaveBeenCalled();
    });
  });

  describe('Edit modal', () => {
    it('openEditModal should set editingCategory and show modal', () => {
      component.openEditModal(mockCategories[0]);
      expect(component.editingCategory()).toEqual(mockCategories[0]);
      expect(component.showEditModal()).toBe(true);
      expect(component.editForm().name).toBe('Electronics');
    });

    it('closeEditModal should hide modal and clear editingCategory', () => {
      component.openEditModal(mockCategories[0]);
      component.closeEditModal();
      expect(component.showEditModal()).toBe(false);
      expect(component.editingCategory()).toBeNull();
    });

    it('saveCategory should call updateCategory API on valid input', () => {
      updateCategory.mockReturnValue(of({ data: { isSuccess: true } }));
      component.openEditModal(mockCategories[0]);
      component.editForm.set({ name: 'Updated Electronics' });
      component.saveCategory();
      expect(updateCategory).toHaveBeenCalledWith('c1', 'Updated Electronics');
    });

    it('saveCategory should show error when name is empty', () => {
      component.openEditModal(mockCategories[0]);
      component.editForm.set({ name: '' });
      component.saveCategory();
      expect(updateCategory).not.toHaveBeenCalled();
    });

    it('saveCategory should show error when name already exists', () => {
      component.openEditModal(mockCategories[0]);
      component.editForm.set({ name: 'Clothing' });
      component.saveCategory();
      expect(updateCategory).not.toHaveBeenCalled();
    });
  });

  describe('Delete', () => {
    it('getDeleteCategoryId should set id and show confirm modal', () => {
      component.getDeleteCategoryId('c1');
      expect(component.deleteCategoryId()).toBe('c1');
      expect(component.confirmModal()).toBe(true);
    });

    it('cancelDelete should clear id and hide modal', () => {
      component.getDeleteCategoryId('c1');
      component.cancelDelete();
      expect(component.deleteCategoryId()).toBe('');
      expect(component.confirmModal()).toBe(false);
    });

    it('deleteCategory should call API and update store on success', () => {
      deleteCategory.mockReturnValue(of({ data: { isSuccess: true }, message: 'Deleted' }));
      component.deleteCategoryId.set('c1');
      component.deleteCategory();
      expect(deleteCategory).toHaveBeenCalledWith('c1');
      expect(setCategories).toHaveBeenCalled();
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

    it('goToPage should set currentPage when data exists', () => {
      component.goToPage(1);
      expect(component.currentPage()).toBe(1);
    });
  });

  describe('Helpers', () => {
    it('totalProducts should sum productsCount', () => {
      expect(component.totalProducts(mockCategories)).toBe(8);
    });

    it('avgProducts should return average products per category', () => {
      expect(component.avgProducts(mockCategories)).toBe('4.0');
    });

    it('avgProducts should return 0 for empty array', () => {
      expect(component.avgProducts([])).toBe('0');
    });
  });
});
