import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DashboardStats } from '../../../models/users/admin.model';
import { StoreService } from '../../../services/adminServices/store.service';
import { UserServcie } from '../../../services/adminServices/user.service';
import { AdminOrderService } from '../../../services/adminServices/orders.service';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { AdminProductService } from '../../../services/adminServices/products.service';
import { PaginationModel } from '../../../models/admin/pagination.model';
import { ApiResponse } from '../../../models/users/apiResponse.model';
import { GetUsersResponseDTO } from '../../../models/admin/users.model';
import { GetAllOrderResponseDTO } from '../../../models/admin/orders.model';
import { GetAllCategoryResponseDTO } from '../../../models/admin/categories.model';
import { GetAllProductsResponseDTO } from '../../../models/admin/products.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-dashboard-overview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-overview.html',
  styleUrl: './dashboard-overview.css',
})
export class DashboardOverview implements OnInit, OnDestroy {
  private readonly userApiService = inject(UserServcie);
  private readonly orderApiService = inject(AdminOrderService);
  private readonly categoryApiService = inject(AdminCategoryService);
  private readonly productApiService = inject(AdminProductService);
  store = inject(StoreService);
  
  private readonly PAGE_SIZE = 10;
  pagination: PaginationModel;

  isLoading = signal<boolean>(false);
  loadingCount = 0;

  stats = signal<DashboardStats>({
    totalUsers: 0,
    totalOrders: 0,
    totalProducts: 0,
    totalRevenue: 0,
    pendingOrders: 0,
    activeUsers: 0,
  });

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = this.PAGE_SIZE;
  }

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadDashboardData();
    this.store.refresh$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.store.resetCache();
      this.loadDashboardData();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDashboardData(): void {
    this.loadingCount = 0;
    this.isLoading.set(true);
    this.getAllUser();
    this.getAllOrders();
    this.getAllCategories();
    this.getAllProducts();
  }

  private onRequestComplete(): void {
    this.loadingCount++;
    if (this.loadingCount >= 4) {
      this.isLoading.set(false);
    }
  }

  getAllProducts(): void {
    this.productApiService.getAllProducts(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllProductsResponseDTO>) => {
        const list = response.data?.productList ?? [];
        this.store.setProducts(list);
        this.store.pageCache.products.add(1);
        this.updateStats();
        this.onRequestComplete();
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load products');
        this.onRequestComplete();
      },
    });
  }

  getAllUser(): void {
    this.userApiService.getAllUser(this.pagination).subscribe({
      next: (response: ApiResponse<GetUsersResponseDTO>) => {
        const list = response.data?.usersList ?? [];
        this.store.setUsers(list);
        this.store.pageCache.users.add(1);
        this.updateStats();
        this.onRequestComplete();
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load users');
        this.onRequestComplete();
      },
    });
  }

  getAllOrders(): void {
    this.orderApiService.getAllOrders(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllOrderResponseDTO>) => {
        const list = response.data?.items ?? [];
        this.store.setOrders(list);
        this.store.pageCache.orders.add(1);
        this.updateStats();
        this.onRequestComplete();
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load orders');
        this.onRequestComplete();
      },
    });
  }

  getAllCategories(): void {
    this.categoryApiService.getAllCategories(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllCategoryResponseDTO>) => {
        const list = response.data?.categoryList ?? [];
        this.store.setCategories(list);
        this.store.pageCache.categories.add(1);
        this.onRequestComplete();
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load categories');
        this.onRequestComplete();
      },
    });
  }

  private updateStats(): void {
    const { users, orders, products } = this.store.value;

    this.stats.set({
      totalUsers: users.length,
      activeUsers: users.length,
      totalOrders: orders.length,
      pendingOrders: orders.filter((o) => o.status === 'Not Delivered').length,
      totalProducts: products.length,
      totalRevenue: orders.reduce((sum, o) => sum + (o.totalAmount ?? 0), 0),
    });
  }
}
