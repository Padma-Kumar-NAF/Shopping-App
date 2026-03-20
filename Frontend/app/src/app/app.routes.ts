import { Routes } from '@angular/router';
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
import { UnauthorizedComponent } from './components/unauthorized/unauthorized';

export const routes: Routes = [
  {
    path: '',
    canActivate: [roleGuard],
    data: { role: 'user' },
    component: HomeComponent,
  },
  {
    path: 'admin',
    canActivate: [roleGuard],
    data: { role: 'admin' },
    component: AdminDashboard,
  },
  {
    path: 'products',
    canActivate: [roleGuard],
    data: { role: 'user' },
    component: ProductListing,
  },
  {
    path: 'product/:id',
    canActivate: [roleGuard],
    data: { role: 'user' },
    component: ProductDetail,
  },
  {
    path: 'checkout',
    canActivate: [roleGuard],
    data: { role: 'user' },
    component: CheckoutComponent,
  },
  {
    path: 'profile',
    canActivate: [roleGuard],
    data: { role: 'user' },
    component: Profile,
    children: [
      { path: 'orders', component: OrdersComponent },
      { path: 'address', component: Address },
      { path: 'cart', component: Cart },
      { path: 'wishlist', component: WishlistComponent },
      {
        path: '',
        redirectTo: 'profile',
        pathMatch: 'full',
      },
    ],
  },
  { path: 'auth', component: Auth },
  { path: 'unauthorized', component: UnauthorizedComponent },
  { path: 'game', component: Game },
  { path: '**', redirectTo: '' },
];

// import { Routes } from '@angular/router';
// import { Profile } from './components/profileComponents/profile/profile';
// import { Auth } from './components/auth/auth';
// import { Game } from './game/game';
// import { roleGuard } from './services/auth.guard';
// import { OrdersComponent } from './components/profileComponents/orders/orders';
// import { Address } from './components/profileComponents/address/address';
// import { WishlistComponent } from './components/profileComponents/wishlist/wishlist';
// import { Cart } from './components/profileComponents/cart/cart';
// import { ProductListing } from './components/product-listing/product-listing';
// import { AdminDashboard } from './components/adminComponents/admin-dashboard/admin-dashboard';
// import { HomeComponent } from './components/homeComponents/home/home';
// import { ProductDetail } from './components/product-detail/product-detail';

// export const routes: Routes = [
//   {
//     path: '',
//     canActivate: [roleGuard],
//     data: { role: 'user' },
//     component : HomeComponent
//     // loadComponent: () =>import('./components/homeComponents/home/home').then((h) => h.HomeComponent),
//   },
//   {
//     path: 'admin',
//     canActivate: [roleGuard],
//     data: { role: 'admin' },
//     component : AdminDashboard
//     // loadComponent: () => import('./components/adminComponents/admin-dashboard/admin-dashboard').then((c) => c.AdminDashboard),
//   },
//   {
//     path: 'products',
//     canActivate: [roleGuard],
//     data: { role: 'user' },
//     component : ProductListing
//     // loadComponent: () => import('./components/product-listing/product-listing').then((p) => p.ProductListing),
//   },
//   {
//     path: 'product/:id',
//     canActivate: [roleGuard],
//     data: { role: 'user', prerender: false },
//     component : ProductDetail
//     // loadComponent: () =>import('./components/product-detail/product-detail').then((p) => p.ProductDetail),
//   },
//   {
//     path: 'profile',
//     component: Profile,
//     canActivate: [roleGuard],
//     children: [
//       {
//         path: 'orders',
//         component: OrdersComponent,
//       },
//       {
//         path: 'address',
//         component: Address,
//       },
//       {
//         path: 'cart',
//         component: Cart,
//       },
//       {
//         path: 'wishlist',
//         component: WishlistComponent,
//       },
//       {
//         path: '',
//         redirectTo: 'profile',
//         pathMatch: 'full',
//       },
//     ],
//   },
//   { path: 'auth', component: Auth },
//   { path: 'game', component: Game },
//   { path: '**', redirectTo: '' },
// ];
