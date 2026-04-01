import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { vi } from 'vitest';
import { DashboardOverview } from './dashboard-overview';
import { StoreService } from '../../../services/adminServices/store.service';
import { UserServcie } from '../../../services/adminServices/user.service';
import { AdminOrderService } from '../../../services/adminServices/orders.service';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { AdminProductService } from '../../../services/adminServices/products.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('DashboardOverview', () => {
  let component: DashboardOverview;
  let fixture: ComponentFixture<DashboardOverview>;
  let refresh$: Subject<void>;

  const getAllUser = vi.fn();
  const getAllOrders = vi.fn();
  const getAllCategories = vi.fn();
  const getAllProducts = vi.fn();
  const setUsers = vi.fn();
  const setOrders = vi.fn();
  const setCategories = vi.fn();
  const setProducts = vi.fn();
  const resetCache = vi.fn();

  const mockUsers = [{ userId: 'u1', name: 'User 1', activeStatus: true }];
  const mockOrders = [
    { orderId: 'o1', status: 'Not Delivered', totalAmount: 5000 },
    { orderId: 'o2', status: 'Delivered', totalAmount: 3000 },
  ];
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
      imports: [DashboardOverview],
      providers: [
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
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call all API methods on init', () => {
    expect(getAllUser).toHaveBeenCalled();
    expect(getAllOrders).toHaveBeenCalled();
    expect(getAllCategories).toHaveBeenCalled();
    expect(getAllProducts).toHaveBeenCalled();
  });

  it('should set isLoading to false after all requests complete', () => {
    expect(component.isLoading()).toBe(false);
  });

  it('should update stats after loading data', () => {
    const stats = component.stats();
    expect(stats.totalUsers).toBe(mockUsers.length);
    expect(stats.totalOrders).toBe(mockOrders.length);
    expect(stats.totalProducts).toBe(mockProducts.length);
    expect(stats.pendingOrders).toBe(1);
    expect(stats.totalRevenue).toBe(8000);
  });

  it('should reload data when refresh$ emits', () => {
    const callsBefore = getAllUser.mock.calls.length;
    refresh$.next();
    expect(resetCache).toHaveBeenCalled();
    expect(getAllUser.mock.calls.length).toBeGreaterThan(callsBefore);
  });

  describe('ngOnDestroy', () => {
    it('should complete destroy$ on destroy', () => {
      const spy = vi.spyOn((component as any).destroy$, 'next');
      component.ngOnDestroy();
      expect(spy).toHaveBeenCalled();
    });
  });
});
