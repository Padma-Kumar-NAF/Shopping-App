import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UsersManagement } from '../users-management/users-management';
import { OrdersManagement } from '../orders-management/orders-management';
import { AddProduct } from '../add-product/add-product';
import { CategoryManagement } from '../category-management/category-management';
import { ProductManagement } from '../product-management/product-management';
import { DashboardStats } from '../../../models/users/admin.model';
import { StoreService } from '../../../services/adminServices/store.service';
import { UserServcie } from '../../../services/adminServices/user.service';
import { PaginationModel } from '../../../models/admin/pagination.model';
import { ApiResponse } from '../../../models/users/apiResponse.model';
import { GetUsersResponseDTO } from '../../../models/admin/users.model';
import { AuthStateService } from '../../../services/auth-state.service';
import { toast } from 'ngx-sonner';
import { Router } from '@angular/router';
import { AdminOrderService } from '../../../services/adminServices/orders.service';
import {
  GetAllOrderResponseDTO,
  OrderDetailsResponseDTO,
} from '../../../models/admin/orders.model';
import { GetAllCategoryResponseDTO } from '../../../models/admin/categories.model';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { AdminProductService } from '../../../services/adminServices/products.service';
import { GetAllProductsResponseDTO } from '../../../models/admin/products.model';

type AdminTab = 'dashboard' | 'users' | 'orders' | 'products' | 'manage-products' | 'categories';

interface TabConfig {
  id: AdminTab;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    UsersManagement,
    OrdersManagement,
    AddProduct,
    CategoryManagement,
    ProductManagement,
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard implements OnInit {
  private readonly userApiService = inject(UserServcie);
  private readonly orderApiService = inject(AdminOrderService);
  private readonly categoryApiService = inject(AdminCategoryService);
  private readonly productApiService = inject(AdminProductService);
  store = inject(StoreService);

  activeTab = signal<AdminTab>('dashboard');
  mobileMenuOpen = signal<boolean>(false);

  pagination: PaginationModel;
  private authState: AuthStateService = inject(AuthStateService);

  constructor(private router: Router) {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = 10;
  }

  ngOnInit(): void {
    console.log('Admin dashboard mounted');
    this.getAllUser();
    this.getAllOrders();
    this.getAllCategories();
    this.getAllProdcuts();
  }

  getAllProdcuts(): void {
    this.productApiService.getAllProducts(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllProductsResponseDTO>) => {
        console.log('response');
        console.log(response);
        this.store.setProducts(response.data?.productList ?? []);
      },
      error: (err) => {
        console.error(err);
        console.error(err?.error?.message);
      },
      complete() {
        console.log('get all products completed');
      },
    });
  }

  getAllUser(): void {
    this.userApiService.getAllUser(this.pagination).subscribe({
      next: (response: ApiResponse<GetUsersResponseDTO>) => {
        console.log('response');
        console.log(response);
        this.store.setUsers(response.data?.usersList ?? []);
      },
      error: (err) => {
        console.error(err);
        console.error(err?.error?.message);
      },
      complete() {
        console.log('get all users completed');
      },
    });
  }

  getAllOrders(): void {
    this.orderApiService.getAllOrders(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllOrderResponseDTO>) => {
        console.log('response');
        console.log(response);
        this.store.setOrders(response.data?.items ?? []);
      },
      error: (err) => {
        console.error(err);
        console.error(err?.error?.message);
      },
      complete() {
        console.log('get all orders completed');
      },
    });
  }

  getAllCategories() {
    this.categoryApiService.getAllCategories(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllCategoryResponseDTO>) => {
        console.log('response', response);
        this.store.setCategories(response.data?.categoryList ?? []);
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load categories');
      },
      complete: () => {
        console.log('get all categories completed');
      },
    });
  }

  tabs: TabConfig[] = [
    { id: 'dashboard', label: 'Dashboard', icon: '📊' },
    { id: 'users', label: 'Users', icon: '👥' },
    { id: 'orders', label: 'Orders', icon: '📦' },
    { id: 'products', label: 'Add Product', icon: '➕' },
    { id: 'manage-products', label: 'Manage Products', icon: '📦' },
    { id: 'categories', label: 'Categories', icon: '🏷️' },
  ];

  stats = signal<DashboardStats>({
    totalUsers: 1248,
    totalOrders: 3567,
    totalProducts: 892,
    totalRevenue: 458920,
    pendingOrders: 45,
    activeUsers: 1102,
  });

  setTab(tab: AdminTab): void {
    this.activeTab.set(tab);
    this.mobileMenuOpen.set(false);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }

  logout(): void {
    console.log('Admin logout');
    this.authState.clearUser();
    toast.success('Logged out successfully');
    this.router.navigate(['/auth']);
  }
}
