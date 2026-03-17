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
  showDetailsModal = signal(false);
  showEmailModal = signal(false);

  passwordVerified = false;

  userDetailsForm: FormGroup;

  editMail: newEmailRequestDTO;
  editUser: EditUserDetailsModel;

  private apiService: ProfileApiService = inject(ProfileApiService);

  activeTab = signal<TabType>('address');
  user: WritableSignal<UserProfile> = signal(new UserProfile());
  isLoading = signal<boolean>(false);

  constructor(private router: Router) {
    this.editUser = new EditUserDetailsModel();
    this.editMail = new newEmailRequestDTO();
    this.userDetailsForm = new FormGroup({
      name: new FormControl('Name', [Validators.required]),
      phoneNumber: new FormControl('9876543210', [Validators.required, Validators.minLength(10)]),
      addressLine1: new FormControl('2/102 A Ragal bavi', [Validators.required]),
      addressLine2: new FormControl('S.K.Palayam(p.o)', [Validators.required]),
      state: new FormControl('Tamil nadu', [Validators.required]),
      city: new FormControl('Udumalpet', [Validators.required]),
      pincode: new FormControl('654321', [Validators.required]),
    });
  }

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

  saveDetails() {
    if (this.userDetailsForm.invalid) {
      this.userDetailsForm.markAllAsTouched();
      toast.error('Please fill all required fields correctly');
      return;
    }

    this.editUser.Details = this.userDetailsForm.value;

    this.apiService.updateUserDetails(this.editUser).subscribe({
      next: (response: any) => {
        console.log('response');
        console.log(response);
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
          },
        }));

        toast.success('Details updated successfully');
        this.closeDetailsModal();
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to update details');
      },
    });
  }

  updateEmail() {
    if (!this.editMail.password?.trim()) {
      toast.error('Password is required');
      return;
    }

    if (!this.editMail.newEmail?.trim()) {
      toast.error('New Email is required');
      return;
    }

    this.editMail.oldEmail = this.user().email;

    console.log('New Email:', this.editMail.newEmail);
    console.log('Password:', this.editMail.password);
    console.log('Old Email:', this.editMail.oldEmail);

    const toastId = toast.loading('Updating email...');

    this.apiService.updateUserEmail(this.editMail).subscribe({
      next: (response: EditMailRequestDTOModel) => {
        toast.dismiss(toastId);

        console.log('Response:', response);

        toast.success('Email updated successfully');

        this.closeEmailModal();
      },

      error: (error: any) => {
        toast.dismiss(toastId);

        console.error('Update Failed:', error);

        const errorMessage = error?.error?.message || error?.error || 'Server error occurred';

        toast.error(errorMessage);
      },
    });
  }

  setTab(tab: TabType): void {
    this.activeTab.set(tab);
  }

  goHome(): void {
    console.log('Navigating back to home');
    this.router.navigate(['/home']);
  }

  logout(): void {
    console.log('Logging out');
    // localStorage.removeItem("JWT-Token")
    toast.success('Logged out successfully');
    this.router.navigate(['/auth']);
  }

  openDetailsModal() {
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
  
  closeDetailsModal() {
    this.showDetailsModal.set(false);
  }

  openEmailModal() {
    this.showEmailModal.set(true);
  }

  closeEmailModal() {
    this.showEmailModal.set(false);
    this.passwordVerified = false;
  }
}
