import { Component, signal, OnInit, inject, WritableSignal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileApiService } from '../../../services/userServices/profile.service';
import { AuthStateService } from '../../../services/auth-state.service';
import { ActivatedRoute } from '@angular/router';
import { NavbarComponent } from '../../shared/navbar/navbar';
import {
  newEmailRequestDTO,
  EditMailResponseDTOModel,
  EditUserDetailsModel,
  GetUserByIdResponseDTO,
} from '../../../models/users/profile.model';
import { toast } from 'ngx-sonner';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import {
  FormControl,
  FormGroup,
  FormsModule,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { ApiResponse } from '../../../models/users/apiResponse.model';
import { WalletService } from '../../../services/userServices/wallet.service';

type TabType = 'cart' | 'wishlist' | 'orders' | 'address' | 'profile';

interface TabConfig {
  id: TabType;
  label: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterOutlet, NavbarComponent],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  showDetailsModal = signal(false);
  showEmailModal = signal(false);
  activeTab = signal<TabType>('profile');
  user: WritableSignal<GetUserByIdResponseDTO> = signal(new GetUserByIdResponseDTO());
  isLoading = signal<boolean>(false);
  mobileMenuOpen = signal(false);
  isMobile = signal(window.innerWidth < 640);

  @HostListener('window:resize')
  onResize() {
    this.isMobile.set(window.innerWidth < 640);
    if (!this.isMobile()) this.mobileMenuOpen.set(false);
  }

  activeTabConfig(): TabConfig {
    return this.tabs.find(t => t.id === this.activeTab()) ?? this.tabs[0];
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  passwordVerified = false;
  userDetailsForm: FormGroup;
  editMail: newEmailRequestDTO;
  editUser: EditUserDetailsModel;

  private route = inject(ActivatedRoute);
  private apiService: ProfileApiService = inject(ProfileApiService);
  private authState: AuthStateService = inject(AuthStateService);
  private walletService = inject(WalletService);

  walletBalance = signal<number | null>(null);
  isLoadingWallet = signal(false);

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

  ngOnInit(): void {
    this.GetUserProfileDetails();

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        this.syncTabFromRoute();
      });

    this.syncTabFromRoute();
  }

  private syncTabFromRoute(): void {
    const child = this.route.firstChild;
    if (child) {
      child.url.subscribe((segments) => {
        if (segments.length > 0) {
          this.activeTab.set(segments[0].path as TabType);
        } else {
          this.activeTab.set('profile');
        }
      });
    } else {
      this.activeTab.set('profile');
    }
  }

  GetUserProfileDetails(): void {    this.isLoading.set(true);
    this.apiService.GetUserProfile().subscribe({
      next: (response: ApiResponse<GetUserByIdResponseDTO>) => {
        this.user.set(response!.data!);
        this.isLoading.set(false);
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

  closeDetailsModal(): void {
    this.showDetailsModal.set(false);
  }

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

  openEmailModal(): void {
    this.showEmailModal.set(true);
  }

  closeEmailModal(): void {
    this.showEmailModal.set(false);
    this.passwordVerified = false;
    this.editMail = new newEmailRequestDTO();
  }

  updateEmail(): void {
    if (!this.editMail.password?.trim()) {
      toast.error('Password is required');
      return;
    }
    if (!this.editMail.newEmail?.trim()) {
      toast.error('New Email is required');
      return;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.editMail.newEmail)) {
      toast.error('Please enter a valid email address');
      return;
    }

    this.editMail.oldEmail = this.user().email;
    const toastId = toast.loading('Updating email...');

    this.apiService.updateUserEmail(this.editMail).subscribe({
      next: (response: ApiResponse<EditMailResponseDTOModel>) => {
        toast.dismiss(toastId);
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

  setTab(tab: TabType): void {
    if (tab === 'profile') {
      this.activeTab.set(tab);
      this.router.navigate(['/profile']);
      return;
    }
    this.activeTab.set(tab);
    this.router.navigate(['/profile', tab]);
  }

  logout(): void {
    this.authState.logout();
    toast.success('Logged out successfully');
  }
}