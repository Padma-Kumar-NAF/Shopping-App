import { Routes } from '@angular/router';
import { HomeComponent } from './components/homeComponents/home/home';
import { ProductListing } from './components/product-listing/product-listing';
import { ProductDetail } from './components/product-detail/product-detail';
import { Profile } from './components/profileComponents/profile/profile';
import { AdminDashboard } from './components/adminComponents/admin-dashboard/admin-dashboard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'products', component: ProductListing },
  {
    path: 'product/:id',
    component: ProductDetail,
    // Disable prerendering for dynamic routes
    data: { prerender: false },
  },
  { path: 'profile', component: Profile },
  { path: 'admin', component: AdminDashboard },
  { path: '**', redirectTo: '' },
];
