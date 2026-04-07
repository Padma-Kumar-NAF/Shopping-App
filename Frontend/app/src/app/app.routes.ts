import { Routes } from '@angular/router';
import { authRequiredGuard, publicGuard } from './core/guards/public-auth.guard';
import { roleGuard } from './core/guards/auth.guard';

import { HomeComponent } from './shared/components/home/home';
import { AdminLayout } from './features/admin/components/admin-layout/admin-layout';
import { DashboardOverview } from './features/admin/components/dashboard-overview/dashboard-overview';
import { UsersManagement } from './features/admin/components/users-management/users-management';
import { OrdersManagement } from './features/admin/components/orders-management/orders-management';
import { ProductManagement } from './features/admin/components/product-management/product-management';
import { CategoryManagement } from './features/admin/components/category-management/category-management';
import { PromocodeManagement } from './features/admin/components/promocode-management/promocode-management';
import { ErrorLogs } from './features/admin/components/error-logs/error-logs';
import { ProductLimit } from './features/admin/components/product-limit/product-limit';
import { ProductListing } from './features/products/components/product-listing/product-listing';
import { ProductDetail } from './features/products/components/product-detail/product-detail';
import { Profile } from './features/user/components/profile/profile';
import { OrdersComponent } from './features/user/components/orders/orders';
import { Address } from './features/user/components/address/address';
import { Cart } from './features/user/components/cart/cart';
import { WishlistComponent } from './features/user/components/wishlist/wishlist';
import { Auth } from './features/auth/auth';
import { PaymentComponent } from './features/payment/payment';
import { UnauthorizedComponent } from './shared/components/unauthorized/unauthorized';
import { PageNotFound } from './shared/components/page-not-found/page-not-found';

export const routes: Routes = [
  {
    path: '',
    canActivate: [publicGuard],
    component: HomeComponent,
  },
  {
    path: 'products',
    canActivate: [publicGuard],
    component: ProductListing,
  },
  {
    path: 'product-detail/:productId',
    canActivate: [publicGuard],
    component: ProductDetail,
  },

  {
    path: 'admin',
    canActivate: [roleGuard],
    data: { role: 'admin' },
    component: AdminLayout,
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: DashboardOverview,
      },
      {
        path: 'users',
        component: UsersManagement,
      },
      {
        path: 'orders',
        component: OrdersManagement,
      },
      {
        path: 'products',
        component: ProductManagement,
      },
      {
        path: 'categories',
        component: CategoryManagement,
      },
      {
        path: 'promocodes',
        component: PromocodeManagement,
      },
      {
        path: 'error-logs',
        component: ErrorLogs,
      },
      {
        path: 'product-limit',
        component: ProductLimit,
      },
    ],
  },
  {
    path: 'payment',
    canActivate: [authRequiredGuard],
    data: { role: 'user' },
    component: PaymentComponent,
  },

  {
    path: 'profile',
    canActivate: [authRequiredGuard],
    canActivateChild: [authRequiredGuard],
    component: Profile,
    children: [
      { path: 'cart', component: Cart },
      { path: 'wishlist', component: WishlistComponent },
      { path: 'orders', component: OrdersComponent },
      { path: 'address', component: Address },
    ],
  },
  { path: 'auth', component: Auth },
  { path: 'unauthorized', component: UnauthorizedComponent },
  { path: 'page-not-found', component: PageNotFound },
  { path: '**', redirectTo: 'page-not-found' },
];
