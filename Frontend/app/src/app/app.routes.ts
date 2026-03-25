import { Routes } from '@angular/router';
import { authRequiredGuard, publicGuard } from './services/public-auth.guard';
import { roleGuard } from './services/auth.guard';

import { HomeComponent } from './components/homeComponents/home/home';
import { AdminLayout } from './components/adminComponents/admin-layout/admin-layout';
import { DashboardOverview } from './components/adminComponents/dashboard-overview/dashboard-overview';
import { UsersManagement } from './components/adminComponents/users-management/users-management';
import { OrdersManagement } from './components/adminComponents/orders-management/orders-management';
import { ProductManagement } from './components/adminComponents/product-management/product-management';
import { CategoryManagement } from './components/adminComponents/category-management/category-management';
import { PromocodeManagement } from './components/adminComponents/promocode-management/promocode-management';
import { ErrorLogs } from './components/adminComponents/error-logs/error-logs';
import { ProductListing } from './components/product-listing/product-listing';
import { ProductDetail } from './components/product-detail/product-detail';
import { Profile } from './components/profileComponents/profile/profile';
import { OrdersComponent } from './components/profileComponents/orders/orders';
import { Address } from './components/profileComponents/address/address';
import { Cart } from './components/profileComponents/cart/cart';
import { WishlistComponent } from './components/profileComponents/wishlist/wishlist';
import { Auth } from './components/auth/auth';
import { Game } from './game/game';
import { CheckoutComponent } from './components/checkout/checkout';
import { PaymentComponent } from './components/payment/payment';
import { UnauthorizedComponent } from './components/unauthorized/unauthorized';
import { PageNotFound } from './components/page-not-found/page-not-found';

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
    ],
  },
  {
    path: 'checkout',
    canActivate: [authRequiredGuard],
    data: { role: 'user' },
    component: CheckoutComponent,
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
  { path: 'game', component: Game },

  { path: '**', redirectTo: 'page-not-found' },
];
