import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

type TabType = 'profile' | 'cart' | 'wishlist' | 'orders' | 'address';

@Component({
  selector: 'app-profile',
  imports: [CommonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {

  activeTab = signal<TabType>('profile');

  user = signal({
    username: 'padma kumar',
    email: 'padmakumar@gmail.com',
    phone: '+91 9876543210',
    addressLine1: '108 / 3 address line 1',
    addressLine2: 'address line 2',
    state: 'Tamil nadu',
    city: 'Chennai',
    pincode: '654321'
  });

  setTab(tab: TabType) {
    this.activeTab.set(tab);
  }

  logout() {
    console.log('Logout clicked');
  }
}
