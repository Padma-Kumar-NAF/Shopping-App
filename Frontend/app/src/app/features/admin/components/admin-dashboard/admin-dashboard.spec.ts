import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { vi } from 'vitest';
import { AdminDashboard } from './admin-dashboard';
import { StoreService } from '../../../services/adminServices/store.service';
import { UserServcie } from '../../../services/adminServices/user.service';
import { AdminOrderService } from '../../../services/adminServices/orders.service';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { AdminProductService } from '../../../services/adminServices/products.service';
import { AuthStateService } from '../../../services/auth-state.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('AdminDashboard', () => {
  let component: AdminDashboard;
  let fixture: ComponentFixture<AdminDashboard>;
  let refresh$: Subject<void>;

  const navigate = vi.fn();
  const getAllUser = vi.fn();
  const getAllOrders = vi.fn();
  const getAllCategories = vi.fn();
  const getAllProducts = vi.fn();
  const logout = vi.fn();
  const setUsers = vi.fn();
  const setOrders = vi.fn();
  const setCategories = vi.fn();
  const setProducts = vi.fn();
  const resetCache = vi.fn();

  const mockUsers = [{ userId: 'u1', name: 'User 1', isActive: true }];
  const mockOrders = [{ orderId: 'o1', status: 'Not Delivered', totalAmount: 5000 }, { orderId: 'o2', status: 'Delivered', totalAmount: 3000 }];
  const mockCategories = [{ categoryId: 'c1', categoryName: 'Electronics' }];
  const mockProducts = [{ productId: 'p1', productName: 'Laptop' }];

  beforeEach(async () => {
    refresh$ = new Subject();
    vi.clearAllMocks();

    getAllUser.mockReturnValue(of({ data: { usersList: mockUsers } }));
    getAllOrders.mockReturnValue(of({ data: { items: mockOrders } }));
    getAllCategories.mockReturnValue(of({ data: { categoryList: mockCategories } }));
    getAllProducts.mockReturnValue(of({ data: { productList: mockProducts } }));

    await TestBed.configureTestingModule({
      imports: [AdminDashboard],
      providers: [
        { provide: Router, useValue: { navigate } },
        {
          provide: StoreService,
          useValue: {
            refresh$: refresh$.asObservable(),
            value: { users: mockUsers, orders: mockOrders, categories: mockCategories, products: mockProducts },
            pageCache: { users: new Set(), orders: new Set(), categories: new Set(), products: new Set() },
            setUsers, setOrders, setCategories, setProducts, resetCache,
          },
        },
        { provide: UserServcie, useValue: { getAllUser } },
        { provide: AdminOrderService, useValue: { getAllOrders } },
        { provide: AdminCategoryService, useValue: { getAllCategories } },
        { provide: AdminProductService, useValue: { getAllProducts } },
        { provide: AuthStateService, useValue: { logout } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with dashboard tab active', () => {
    expect(component.activeTab()).toBe('dashboard');
  });

  it('should call loadAll on init', () => {
    expect(getAllUser).toHaveBeenCalled();
    expect(getAllOrders).toHaveBeenCalled();
    expect(getAllCategories).toHaveBeenCalled();
    expect(getAllProducts).toHaveBeenCalled();
  });

  describe('setTab', () => {
    it('should set activeTab and close mobile menu', () => {
      component.mobileMenuOpen.set(true);
      component.setTab('users');
      expect(component.activeTab()).toBe('users');
      expect(component.mobileMenuOpen()).toBe(false);
    });
  });

  describe('toggleMobileMenu', () => {
    it('should toggle mobileMenuOpen', () => {
      expect(component.mobileMenuOpen()).toBe(false);
      component.toggleMobileMenu();
      expect(component.mobileMenuOpen()).toBe(true);
    });
  });

  describe('loadAll', () => {
    it('should call all API methods', () => {
      component.loadAll();
      expect(getAllUser).toHaveBeenCalledTimes(2);
      expect(getAllOrders).toHaveBeenCalledTimes(2);
    });
  });

  describe('refresh', () => {
    it('should reset cache and reload data', () => {
      component.refresh();
      expect(resetCache).toHaveBeenCalled();
    });
  });

  describe('stats', () => {
    it('should have 5 tabs defined', () => {
      expect(component.tabs.length).toBe(5);
    });

    it('stats signal should be initialized', () => {
      expect(component.stats()).toBeDefined();
    });
  });

  describe('logout', () => {
    it('should call authState.logout', () => {
      component.logout();
      expect(logout).toHaveBeenCalled();
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
