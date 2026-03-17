import { Routes } from '@angular/router';
import { HomeComponent } from './components/homeComponents/home/home';
import { ProductListing } from './components/product-listing/product-listing';
import { ProductDetail } from './components/product-detail/product-detail';
import { Profile } from './components/profileComponents/profile/profile';
import { AdminDashboard } from './components/adminComponents/admin-dashboard/admin-dashboard';
import { Auth } from './components/auth/auth';
import { Game } from './game/game';
import { authGuard } from './services/auth.guard';
import { OrdersComponent } from './components/profileComponents/orders/orders';
import { Address } from './components/profileComponents/address/address';
import { WishlistComponent } from './components/profileComponents/wishlist/wishlist';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'products', component: ProductListing },
  {
    path: 'product/:id',
    component: ProductDetail,
    data: { prerender: false },
  },
  {path:"game",component:Game},
  { path: 'profile', component: Profile,canActivate: [authGuard] ,
    children: [

      {
        path: 'orders',
        component: OrdersComponent
      },
      {
        path: 'address',
        component: Address
      },
      {
        path: 'wishlist',
        component: WishlistComponent
      },
      {
        path: '',
        redirectTo: 'orders',
        pathMatch: 'full'
      }
    ]
  },
  { path: 'admin', component: AdminDashboard },
  { path: 'auth', component: Auth },
  { path: '**', redirectTo: '' },
];