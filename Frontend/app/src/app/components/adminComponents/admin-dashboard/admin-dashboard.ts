import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UsersManagement } from '../users-management/users-management';
import { OrdersManagement } from '../orders-management/orders-management';
import { AddProduct } from '../add-product/add-product';
import { CategoryManagement } from '../category-management/category-management';
import { DashboardStats } from '../../../models/admin.model';

type AdminTab = 'dashboard' | 'users' | 'orders' | 'products' | 'categories';

interface TabConfig {
  id: AdminTab;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, UsersManagement, OrdersManagement, AddProduct, CategoryManagement],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard {
  activeTab = signal<AdminTab>('dashboard');

  tabs: TabConfig[] = [
    { id: 'dashboard', label: 'Dashboard', icon: '📊' },
    { id: 'users', label: 'Users', icon: '👥' },
    { id: 'orders', label: 'Orders', icon: '📦' },
    { id: 'products', label: 'Add Product', icon: '➕' },
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
  }

  logout(): void {
    console.log('Admin logout');
  }
}
