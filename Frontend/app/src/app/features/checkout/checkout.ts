import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../services/userServices/cart.service';
import { AddressSelectionService } from '../../services/address-selection.service';
import { AddressApiService } from '../../services/userServices/address.service';
import { AuthStateService } from '../../services/auth-state.service';
import { AddressDTO } from '../../models/users/address.model';
import { PaginationModel } from '../../models/users/pagination.model';
import { CartItemDTO, OrderAllFromCartRequestDTO } from '../../models/users/cart.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class CheckoutComponent implements OnInit {
  private router = inject(Router);
  private cartService = inject(CartService);
  private addressSelectionService = inject(AddressSelectionService);
  private addressApiService = inject(AddressApiService);
  private authStateService = inject(AuthStateService);

  cartId = signal<string>('');
  cartItems = signal<CartItemDTO[]>([]);
  isLoading = signal<boolean>(false);
  selectedPaymentMethod = signal<string>('card');
  isProcessing = signal<boolean>(false);

  cardNumber = signal<string>('');
  cardName = signal<string>('');
  expiryDate = signal<string>('');
  cvv = signal<string>('');
  upiId = signal<string>('');

  availableAddresses = signal<AddressDTO[]>([]);
  selectedAddress = signal<AddressDTO | null>(null);
  showAddressPicker = signal<boolean>(false);

  ngOnInit(): void {
    this.loadCart();
    this.loadAddresses();
  }

  private loadCart(): void {
    this.isLoading.set(true);
    const pagination = new PaginationModel();
    pagination.pageSize = 100;
    pagination.pageNumber = 1;
    this.cartService.GetUserCart(pagination).subscribe({
      next: (res) => {
        this.cartId.set(res.data?.cartId ?? '');
        this.cartItems.set(res.data?.cartItems ?? []);
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to load cart');
        this.isLoading.set(false);
      },
    });
  }

  private loadAddresses(): void {
    const cached = this.addressSelectionService.getAvailableAddresses();
    if (cached.length > 0) {
      this.availableAddresses.set(cached);
      const selected = this.addressSelectionService.getSelectedAddress();
      if (selected) this.selectedAddress.set(selected);
      return;
    }
    const pagination = new PaginationModel();
    pagination.pageSize = 10;
    pagination.pageNumber = 1;
    this.addressApiService.GetUserAddresses(pagination).subscribe({
      next: (res) => {
        if (res?.data?.addressList) {
          this.availableAddresses.set(res.data.addressList);
          this.addressSelectionService.setAvailableAddresses(res.data.addressList);
        }
      },
    });
  }

  selectAddress(addr: AddressDTO): void {
    this.selectedAddress.set(addr);
    this.addressSelectionService.setSelectedAddress(addr);
    this.showAddressPicker.set(false);
  }

  get subtotal(): number {
    return this.cartItems().reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  get tax(): number { return this.subtotal * 0.18; }
  get shipping(): number { return this.subtotal > 5000 ? 0 : 99; }
  get total(): number { return this.subtotal + this.tax + this.shipping; }

  selectPaymentMethod(method: string): void {
    this.selectedPaymentMethod.set(method);
  }

  processPayment(): void {
    if (!this.selectedAddress()) {
      toast.error('Please select a delivery address');
      return;
    }

    const method = this.selectedPaymentMethod();
    if (method === 'card') {
      if (!this.cardNumber() || !this.cardName() || !this.expiryDate() || !this.cvv()) {
        toast.error('Please fill all card details');
        return;
      }
    } else if (method === 'upi') {
      if (!this.upiId()) { toast.error('Please enter UPI ID'); return; }
    }

    if (!this.cartId()) { toast.error('Cart not loaded'); return; }

    this.isProcessing.set(true);
    const toastId = toast.loading('Placing order...');

    const request = new OrderAllFromCartRequestDTO();
    request.cartId = this.cartId();
    request.addressId = this.selectedAddress()!.addressId;
    request.paymentType = method;

    this.cartService.orderAllFromCart(request).subscribe({
      next: (res) => {
        this.isProcessing.set(false);
        toast.dismiss(toastId);
        if (res.data?.isSuccess) {
          toast.success('Order placed successfully!');
          this.router.navigate(['/profile/orders']);
        } else {
          toast.error(res.message || 'Failed to place order');
        }
      },
      error: (err) => {
        this.isProcessing.set(false);
        toast.dismiss(toastId);
        toast.error(err?.error?.message || 'Failed to place order');
      },
    });
  }
}
