import { Component, signal, OnInit, inject, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Cart } from '../cart/cart';
import { WishlistComponent } from '../wishlist/wishlist';
import { OrdersComponent } from '../orders/orders';
import { Address } from '../address/address';
import { ProfileApiService } from '../../../services/profile.service';
import {
  newEmailRequestDTO,
  EditMailRequestDTOModel,
  EditUserDetailsModel,
  UserProfile,
} from '../../../models/profile.model';
import { toast } from 'ngx-sonner';
import { Router, RouterOutlet } from '@angular/router';
import {
  FormControl,
  FormGroup,
  FormsModule,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';

import { Subject } from 'rxjs';
import { Observable } from 'rxjs';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';

type TabType = 'profile' | 'cart' | 'wishlist' | 'orders' | 'address';

interface TabConfig {
  id: TabType;
  label: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    Cart,
    WishlistComponent,
    OrdersComponent,
    Address,
    FormsModule,
    ReactiveFormsModule,
    RouterOutlet
  ],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  // ==================== Signals ====================
  showDetailsModal = signal(false);
  showEmailModal = signal(false);
  showMobileMenu = signal(false);
  activeTab = signal<TabType>('profile');
  user: WritableSignal<UserProfile> = signal(new UserProfile());
  isLoading = signal<boolean>(false);

  // ==================== Form Properties ====================
  passwordVerified = false;
  userDetailsForm: FormGroup;
  editMail: newEmailRequestDTO;
  editUser: EditUserDetailsModel;

  // ==================== Dependency Injection ====================
  private apiService: ProfileApiService = inject(ProfileApiService);

  // ==================== Tab Configuration ====================
  tabs: TabConfig[] = [
    { id: 'profile', label: 'Profile', icon: '👤', color: 'blue' },
    { id: 'cart', label: 'Cart', icon: '🛒', color: 'green' },
    { id: 'wishlist', label: 'Wishlist', icon: '❤️', color: 'red' },
    { id: 'orders', label: 'Orders', icon: '📦', color: 'purple' },
    { id: 'address', label: 'Address', icon: '📍', color: 'orange' },
  ];

  constructor(private router: Router) {
    this.editUser = new EditUserDetailsModel();
    this.editMail = new newEmailRequestDTO();
    this.userDetailsForm = new FormGroup({
      name: new FormControl('', [Validators.required]),
      phoneNumber: new FormControl('', [Validators.required, Validators.minLength(10)]),
      addressLine1: new FormControl('', [Validators.required]),
      addressLine2: new FormControl(''),
      state: new FormControl('', [Validators.required]),
      city: new FormControl('', [Validators.required]),
      pincode: new FormControl('', [Validators.required]),
    });
  }

  // ==================== Lifecycle ====================
  ngOnInit(): void {
    this.GetUserProfileDetails();
  }

  // ==================== Mobile Menu ====================
  /**
   * Toggle mobile menu visibility
   */
  toggleMobileMenu(): void {
    this.showMobileMenu.update(value => !value);
  }

  /**
   * Close mobile menu
   */
  closeMobileMenu(): void {
    this.showMobileMenu.set(false);
  }

  // ==================== User Profile ====================
  /**
   * Fetch user profile details from API
   */
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

  // ==================== Details Modal ====================
  /**
   * Open details modal with current user data
   */
  openDetailsModal(): void {
    const user = this.user();

    this.userDetailsForm.patchValue({
      name: user.name,
      phoneNumber: user.userDetails?.phoneNumber,
      addressLine1: user.userDetails?.addressLine1,
      addressLine2: user.userDetails?.addressLine2,
      state: user.userDetails?.state,
      city: user.userDetails?.city,
      pincode: user.userDetails?.pincode,
    });

    this.showDetailsModal.set(true);
  }

  /**
   * Close details modal
   */
  closeDetailsModal(): void {
    this.showDetailsModal.set(false);
    this.showMobileMenu.set(false);
  }

  /**
   * Save user details
   */
  saveDetails(): void {
    if (this.userDetailsForm.invalid) {
      this.userDetailsForm.markAllAsTouched();
      toast.error('Please fill all required fields correctly');
      return;
    }

    const toastId = toast.loading('Saving details...');
    this.editUser.Details = this.userDetailsForm.value;

    this.apiService.updateUserDetails(this.editUser).subscribe({
      next: (response: any) => {
        toast.dismiss(toastId);
        console.log('Details updated:', response);

        this.user.update((user) => ({
          ...user,
          name: this.editUser.Details.name,
          userDetails: {
            ...user.userDetails,
            addressLine1: this.editUser.Details.addressLine1,
            addressLine2: this.editUser.Details.addressLine2,
            state: this.editUser.Details.state,
            city: this.editUser.Details.city,
            phoneNumber: this.editUser.Details.phoneNumber,
            pincode: this.editUser.Details.pincode,
          },
        }));

        toast.success('Details updated successfully');
        this.closeDetailsModal();
      },
      error: (err) => {
        toast.dismiss(toastId);
        console.error('Update failed:', err);
        toast.error('Failed to update details');
      },
    });
  }

  // ==================== Email Modal ====================
  /**
   * Open email modal
   */
  openEmailModal(): void {
    this.showEmailModal.set(true);
  }

  /**
   * Close email modal
   */
  closeEmailModal(): void {
    this.showEmailModal.set(false);
    this.passwordVerified = false;
    this.editMail = new newEmailRequestDTO();
    this.showMobileMenu.set(false);
  }

  /**
   * Update user email
   */
  updateEmail(): void {
    if (!this.editMail.password?.trim()) {
      toast.error('Password is required');
      return;
    }

    if (!this.editMail.newEmail?.trim()) {
      toast.error('New Email is required');
      return;
    }

    // Email format validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.editMail.newEmail)) {
      toast.error('Please enter a valid email address');
      return;
    }

    this.editMail.oldEmail = this.user().email;

    const toastId = toast.loading('Updating email...');

    this.apiService.updateUserEmail(this.editMail).subscribe({
      next: (response: EditMailRequestDTOModel) => {
        toast.dismiss(toastId);
        console.log('Email updated:', response);

        this.user.update((user) => ({
          ...user,
          email: this.editMail.newEmail || user.email,
        }));

        toast.success('Email updated successfully');
        this.closeEmailModal();
      },
      error: (error: any) => {
        toast.dismiss(toastId);
        console.error('Update failed:', error);

        const errorMessage = error?.error?.message || error?.error || 'Server error occurred';
        toast.error(errorMessage);
      },
    });
  }

  // ==================== Tab Management ====================
  /**
   * Set active tab
   */
  setTab(tab: TabType): void {
    this.activeTab.set(tab);
  }

  // ==================== Navigation ====================
  /**
   * Navigate back to home
   */
  goHome(): void {
    console.log('Navigating back to home');
    this.closeMobileMenu();
    this.router.navigate(['/home']);
  }

  /**
   * Logout user
   */
  logout(): void {
    console.log('Logging out');
    // localStorage.removeItem("JWT-Token")
    toast.success('Logged out successfully');
    this.router.navigate(['/auth']);
  }
}