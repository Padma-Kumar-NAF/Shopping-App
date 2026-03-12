import { Component, signal, OnInit, inject, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Cart } from '../cart/cart';
import { WishlistComponent } from '../wishlist/wishlist';
import { OrdersComponent } from '../orders/orders';
import { Address } from '../address/address';
import { ProfileApiService } from '../../../services/profile.service';
import { UserProfile } from '../../../models/profile.model';
import { toast } from 'ngx-sonner';

type TabType = 'profile' | 'cart' | 'wishlist' | 'orders' | 'address';

interface TabConfig {
  id: TabType;
  label: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-profile',
  imports: [CommonModule, Cart, WishlistComponent, OrdersComponent, Address],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  private apiService: ProfileApiService = inject(ProfileApiService);

  activeTab = signal<TabType>('profile');
  user: WritableSignal<UserProfile> = signal(new UserProfile());
  isLoading = signal<boolean>(false);

  tabs: TabConfig[] = [
    { id: 'profile', label: 'Profile', icon: '👤', color: 'blue' },
    { id: 'cart', label: 'Cart', icon: '🛒', color: 'green' },
    { id: 'wishlist', label: 'Wishlist', icon: '❤️', color: 'red' },
    { id: 'orders', label: 'Orders', icon: '📦', color: 'purple' },
    { id: 'address', label: 'Address', icon: '📍', color: 'orange' },
  ];

  ngOnInit(): void {
    this.GetUserProfileDetails();
  }

  GetUserProfileDetails(): void {
    this.isLoading.set(true);
    this.apiService.GetUserProfile().subscribe({
      next: (response: UserProfile) => {
        this.user.set(response);
        this.isLoading.set(false);
        console.log('User details:', this.user());
      },
      error: (error: any) => {
        this.isLoading.set(false);
        console.error('Failed to load profile:', error);
        if (error.error?.message) {
          toast.error(error.error.message);
        }
      },
    });
  }

  setTab(tab: TabType): void {
    this.activeTab.set(tab);
  }

  goHome(): void {
    console.log('Navigating back to home');
    // Add navigation logic here
  }

  logout(): void {
    console.log('Logging out');
    // localStorage.removeItem("JWT-Token")
    toast.success('Logged out successfully');
  }

  editProfile(): void {
    console.log('Edit profile clicked');
    toast.info('Edit profile feature coming soon');
  }
}
