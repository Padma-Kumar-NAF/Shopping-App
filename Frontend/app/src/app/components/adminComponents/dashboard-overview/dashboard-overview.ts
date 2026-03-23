import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
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
export class DashboardOverview implements OnInit {
  private readonly userApiService = inject(UserServcie);
  private readonly orderApiService = inject(AdminOrderService);
  private readonly categoryApiService = inject(AdminCategoryService);
  private readonly productApiService = inject(AdminProductService);
  store = inject(StoreService);

  private readonly PAGE_SIZE = 10;
  pagination: PaginationModel;

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

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.getAllUser();
    this.getAllOrders();
    this.getAllCategories();
    this.getAllProducts();
  }

  getAllProducts(): void {
    this.productApiService.getAllProducts(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllProductsResponseDTO>) => {
        const list = response.data?.productList ?? [];
        this.store.setProducts(list);
        this.store.pageCache.products.add(1);
        this.updateStats();
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load products');
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
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load users');
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
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load orders');
      },
    });
  }

  getAllCategories(): void {
    this.categoryApiService.getAllCategories(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllCategoryResponseDTO>) => {
        const list = response.data?.categoryList ?? [];
        this.store.setCategories(list);
        this.store.pageCache.categories.add(1);
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load categories');
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
