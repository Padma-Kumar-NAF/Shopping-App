import { Component, signal, OnInit, inject, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Cart } from '../cart/cart';
import { WishlistComponent } from '../wishlist/wishlist';
import { OrdersComponent } from '../orders/orders';
import { Address } from '../address/address';
import { ProfileApiService } from '../../../services/profile.service';
import { UserProfile } from '../../../models/profile.model';
import { toast } from 'ngx-sonner';

type TabType = 'profile' | 'cart' | 'wishlist' | 'orders' | 'address';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, MatIconModule, Cart, WishlistComponent, OrdersComponent, Address],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  private apiService: ProfileApiService = inject(ProfileApiService);
  ngOnInit(): void {
    this.GetUserProfileDetails();
  }

  activeTab = signal<TabType>('profile');

  user: WritableSignal<UserProfile> = signal(new UserProfile());

  GetUserProfileDetails() {
    this.apiService.GetUserProfile().subscribe({
      next: (response: UserProfile) => {
        this.user.set(response);
        console.log('User details');
        console.log(this.user());
      },
      error: (error: any) => {
        console.error('Login Failed:', error);
        if (error.error?.message) {
          toast.error(error.error.message);
        }
      },
    });
  }

  setTab(tab: TabType) {
    this.activeTab.set(tab);
  }

  goHome() {
    console.log('back to home');
  }

  logout() {
    // localStorage.removeItem("JWT-Token")
  }
}
