import { Routes } from '@angular/router';
import { authRequiredGuard, publicGuard } from './services/public-auth.guard';
import { roleGuard } from './services/auth.guard';

import { HomeComponent } from './components/homeComponents/home/home';
import { AdminDashboard } from './components/adminComponents/admin-dashboard/admin-dashboard';
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
  // Public routes - no authentication required
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
    path: 'product/:id',
    canActivate: [publicGuard],
    component: ProductDetail,
  },

  // Admin routes - require admin role
  {
    path: 'admin',
    canActivate: [roleGuard],
    data: { role: 'admin' },
    component: AdminDashboard,
  },

  // Protected routes - require authentication
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

  // Profile routes - require authentication
  {
    path: 'profile',
    canActivate: [authRequiredGuard],
    component: Profile,
    children: [
      { path: 'cart', component: Cart },
      { path: 'wishlist', component: WishlistComponent },
      { path: 'orders', component: OrdersComponent },
      { path: 'address', component: Address },
      { path: '', component: Profile },
      { path: '**', redirectTo: '/profile' },
    ],
  },

  // Auth and utility routes
  { path: 'auth', component: Auth },
  { path: 'unauthorized', component: UnauthorizedComponent },
  { path: 'page-not-found', component: PageNotFound },
  { path: 'game', component: Game },

  { path: '**', redirectTo: 'page-not-found' },
];
