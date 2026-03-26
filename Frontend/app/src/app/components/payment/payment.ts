import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';

declare var Razorpay: any;

import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { CartService } from '../../services/cart.service';
import { AddressSelectionService } from '../../services/address-selection.service';
import { AddressApiService } from '../../services/userServices/address.service';
import { AuthStateService } from '../../services/auth-state.service';
import { OrderService, PlaceOrderRequestDTO } from '../../services/userServices/order.service';
import { PromoCodeService } from '../../services/adminServices/promocode.service';
import { GetWalletAmountResponseDTO, WalletService } from '../../services/userServices/wallet.service';
import { AddressDTO } from '../../models/users/address.model';
import { PaginationModel } from '../../models/users/pagination.model';
import { ProductDetails, SearchProductByIdResponseDTO } from '../../models/users/product.model';
import { OrderAllFromCartRequestDTO } from '../../models/users/cart.model';
import { toast } from 'ngx-sonner';
import { ApiResponse } from '../../models/users/apiResponse.model';

interface CartItem {
  id: string;
  name: string;
  price: number;
  quantity: number;
  image: string;
}

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class PaymentComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private productStateService = inject(ProductStateService);
  private cartService = inject(CartService);
  private addressSelectionService = inject(AddressSelectionService);
  private addressApiService = inject(AddressApiService);
  private authStateService = inject(AuthStateService);
  private orderService = inject(OrderService);
  private promoCodeService = inject(PromoCodeService);
  private walletService = inject(WalletService);

  // ── Payment mode ──────────────────────────────────────────────────────────
  paymentMode = signal<'single' | 'cart'>('single');
  product = signal<ProductDetails | SearchProductByIdResponseDTO | null>(null);
  quantity = signal<number>(1);
  cartItems = signal<CartItem[]>([]);

  // ── Selected top-level method: 'razorpay' | 'wallet' ─────────────────────
  selectedPaymentMethod = signal<'razorpay' | 'wallet'>('razorpay');
  isProcessing = signal<boolean>(false);

  // ── Shipping details ──────────────────────────────────────────────────────
  fullName = signal<string>('');
  email = signal<string>('');
  phone = signal<string>('');
  address = signal<string>('');
  city = signal<string>('');
  state = signal<string>('');
  pincode = signal<string>('');

  availableAddresses = this.addressSelectionService.availableAddresses$;
  selectedAddress = signal<AddressDTO | null>(null);
  showAddressPicker = signal<boolean>(false);

  // ── Promo code ────────────────────────────────────────────────────────────
  promoInput = signal<string>('');
  appliedPromo = signal<string | null>(null);
  discountPercent = signal<number>(0);
  promoError = signal<string | null>(null);
  isValidatingPromo = signal<boolean>(false);

  // ── Wallet ────────────────────────────────────────────────────────────────
  walletBalance = signal<number | null>(null);
  isLoadingWallet = signal<boolean>(false);

  get walletApplied(): number {
    const bal = this.walletBalance();
    if (this.selectedPaymentMethod() !== 'wallet' || bal === null) return 0;
    return Math.min(bal, this.total);
  }

  get remainingAfterWallet(): number {
    return Math.max(0, this.total - this.walletApplied);
  }

  get walletCoversAll(): boolean {
    const bal = this.walletBalance();
    return this.selectedPaymentMethod() === 'wallet' && bal !== null && bal >= this.total;
  }

  // ── Razorpay mock modal ───────────────────────────────────────────────────
  showRazorpayModal = signal<boolean>(false);
  razorpayAmount = signal<number>(0);
  selectedRzpMethod = signal<'card' | 'upi' | 'netbanking'>('card');
  rzpCardNumber = signal<string>('');
  rzpCardName = signal<string>('');
  rzpExpiry = signal<string>('');
  rzpCvv = signal<string>('');
  rzpUpiId = signal<string>('');
  rzpBank = signal<string>('HDFC');
  isRzpProcessing = signal<boolean>(false);

  // ── Totals ────────────────────────────────────────────────────────────────
  get subtotal(): number {
    if (this.paymentMode() === 'cart')
      return this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0);
    return (this.product()?.price || 0) * this.quantity();
  }
  get totalItems(): number {
    if (this.paymentMode() === 'cart')
      return this.cartItems().reduce((s, i) => s + i.quantity, 0);
    return this.quantity();
  }
  get tax(): number { return this.subtotal * 0.18; }
  get shipping(): number { return this.subtotal > 5000 ? 0 : 99; }
  get discountAmount(): number { return (this.subtotal * this.discountPercent()) / 100; }
  get total(): number { return this.subtotal - this.discountAmount + this.tax + this.shipping; }

  // ── resolvedPaymentType for backend ──────────────────────────────────────
  get resolvedPaymentType(): string {
    if (this.selectedPaymentMethod() === 'wallet') {
      if (this.walletCoversAll) return 'wallet';
      return 'wallet+razorpay';
    }
    return 'razorpay';
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────
  ngOnInit(): void {
    const fromCart = this.route.snapshot.queryParamMap.get('fromCart');
    const fromProduct = this.route.snapshot.queryParamMap.get('fromProduct');
    const qty = this.route.snapshot.queryParamMap.get('quantity');

    if (fromCart === 'true') {
      this.paymentMode.set('cart');
      this.loadCartItems();
    } else if (fromProduct === 'true') {
      this.paymentMode.set('single');
      if (qty) this.quantity.set(parseInt(qty));
      this.loadProductFromState();
    } else {
      const productId = this.route.snapshot.queryParamMap.get('productId');
      if (productId) {
        this.paymentMode.set('single');
        if (qty) this.quantity.set(parseInt(qty));
        this.loadSingleProduct(productId);
      } else {
        toast.error('No items to checkout');
        this.router.navigate(['/products']);
      }
    }

    this.loadAddresses();
    this.fullName.set(this.authStateService.username());
    this.email.set(this.authStateService.email());
  }

  // ── Address ───────────────────────────────────────────────────────────────
  private loadAddresses(): void {
    const cached = this.addressSelectionService.getAvailableAddresses();
    if (cached.length === 0) {
      const p = new PaginationModel();
      p.pageSize = 10; p.pageNumber = 1;
      this.addressApiService.GetUserAddresses(p).subscribe({
        next: (r) => { if (r?.data?.addressList) this.addressSelectionService.setAvailableAddresses(r.data.addressList); },
        error: () => { },
      });
    }
    const sel = this.addressSelectionService.getSelectedAddress();
    if (sel) this.applyAddress(sel);
  }

  selectAddress(addr: AddressDTO): void {
    this.addressSelectionService.setSelectedAddress(addr);
    this.applyAddress(addr);
    this.showAddressPicker.set(false);
  }

  unselectAddress(): void {
    this.selectedAddress.set(null);
    this.addressSelectionService.clearSelectedAddress();
    this.address.set(''); this.city.set(''); this.state.set(''); this.pincode.set('');
  }

  private applyAddress(addr: AddressDTO): void {
    this.selectedAddress.set(addr);
    this.address.set(`${addr.addressLine1}${addr.addressLine2 ? ', ' + addr.addressLine2 : ''}`);
    this.city.set(addr.city); this.state.set(addr.state); this.pincode.set(addr.pincode);
  }

  // ── Product / cart loaders ────────────────────────────────────────────────
  private loadProductFromState(): void {
    const p = this.productStateService.getSelectedProduct();
    if (p) { this.product.set(p); return; }
    const productId = this.route.snapshot.queryParamMap.get('productId');
    if (productId) this.loadSingleProduct(productId);
    else { toast.error('Product not found'); this.router.navigate(['/products']); }
  }

  private loadSingleProduct(productId: string): void {
    this.productService.getProductById(productId).subscribe({
      next: (p) => { if (p) this.product.set(p); else { toast.error('Product not found'); this.router.navigate(['/products']); } },
      error: () => { toast.error('Failed to load product'); this.router.navigate(['/products']); },
    });
  }

  private loadCartItems(): void {
    const p = new PaginationModel(); p.pageSize = 100; p.pageNumber = 1;
    this.cartService.GetUserCart(p).subscribe({
      next: (r) => {
        if (r.data?.cartItems)
          this.cartItems.set(r.data.cartItems.map(i => ({ id: i.productId, name: i.productName, price: i.price, quantity: i.quantity, image: i.imagePath })));
      },
      error: (err) => { toast.error('Failed to load cart items'); this.router.navigate(['/profile/cart']); },
    });
  }

  // ── Promo ─────────────────────────────────────────────────────────────────
  applyPromo(): void {
    if (this.appliedPromo() || this.isValidatingPromo()) return;
    const code = this.promoInput().trim().toUpperCase();
    if (!code) { this.promoError.set('Please enter a promo code.'); return; }
    this.isValidatingPromo.set(true); this.promoError.set(null);
    this.promoCodeService.validatePromoCode({ promoCodeName: code }).subscribe({
      next: (res) => {
        this.isValidatingPromo.set(false);
        if (res.data?.isValid) { this.appliedPromo.set(code); this.discountPercent.set(res.data.discountPercentage); toast.success(`${res.data.discountPercentage}% off applied!`); }
        else this.promoError.set(res.message || 'Invalid promo code.');
      },
      error: (err) => { this.isValidatingPromo.set(false); this.promoError.set(err?.error?.message || 'Invalid promo code.'); },
    });
  }

  removePromo(): void { this.appliedPromo.set(null); this.discountPercent.set(0); this.promoInput.set(''); this.promoError.set(null); }

  // ── Wallet selection ──────────────────────────────────────────────────────
  selectMethod(method: 'razorpay' | 'wallet'): void {
    this.selectedPaymentMethod.set(method);
    if (method === 'wallet' && this.walletBalance() === null) this.fetchWalletBalance();
    if (method === 'razorpay') { this.walletBalance.set(null); }
  }

  private fetchWalletBalance(): void {
    this.isLoadingWallet.set(true);
    this.walletService.getWalletBalance().subscribe({
      next: (res: ApiResponse<GetWalletAmountResponseDTO>) => {
        const bal = res.data?.walletBalance ?? 0;
        this.walletBalance.set(bal);
        this.isLoadingWallet.set(false);
        if (bal <= 0) { toast.error('Wallet balance is ₹0.'); this.selectedPaymentMethod.set('razorpay'); this.walletBalance.set(null); }
        else if (bal >= this.total) toast.success(`Wallet (₹${bal.toLocaleString()}) covers the full order.`);
        else toast.info(`₹${bal.toLocaleString()} from wallet. Razorpay will handle the remaining ₹${(this.total - bal).toLocaleString()}.`);
      },
      error: (err) => { toast.error(err?.error?.message || 'Could not fetch wallet balance.'); this.isLoadingWallet.set(false); this.selectedPaymentMethod.set('razorpay'); },
    });
  }

  // ── Quantity ──────────────────────────────────────────────────────────────
  updateQuantity(change: number): void {
    const n = this.quantity() + change;
    if (n >= 1 && n <= (this.product()?.quantity || 1)) this.quantity.set(n);
  }

  // ── Validation ────────────────────────────────────────────────────────────
  private validateShippingDetails(): boolean {
    if (!this.fullName() || !this.email() || !this.phone() || !this.address() || !this.city() || !this.state() || !this.pincode()) {
      toast.error('Please fill all shipping details'); return false;
    }
    return true;
  }

  // ── Razorpay modal ────────────────────────────────────────────────────────
  openRazorpayModal(amount: number): void {
    this.razorpayAmount.set(amount);
    this.rzpCardNumber.set(''); this.rzpCardName.set(''); this.rzpExpiry.set(''); this.rzpCvv.set(''); this.rzpUpiId.set('');
    this.showRazorpayModal.set(true);
  }

  closeRazorpayModal(): void {
    this.showRazorpayModal.set(false);
    this.isRzpProcessing.set(false);
    this.isProcessing.set(false);
    toast.error('Payment Cancelled');
  }

  submitRazorpayPayment(): void {
    this.isRzpProcessing.set(true);
    setTimeout(() => {
      const rzpResponse = {
        razorpay_payment_id: 'pay_' + Math.random().toString(36).substring(2, 12).toUpperCase(),
        razorpay_order_id: 'order_' + Math.random().toString(36).substring(2, 12).toUpperCase(),
        razorpay_signature: 'sig_' + Math.random().toString(36).substring(2, 20),
      };
      console.log('Razorpay Payment Response:', rzpResponse);
      this.isRzpProcessing.set(false);
      this.showRazorpayModal.set(false);
      // Now place the order after successful Razorpay payment
      this.placeOrder(rzpResponse);
    }, 1500);
  }

  // ── Main entry point ──────────────────────────────────────────────────────
  processPayment(): void {
    if (!this.validateShippingDetails()) return;
    const addr = this.selectedAddress();
    if (!addr) { toast.error('Please select a delivery address'); return; }

    const method = this.selectedPaymentMethod();

    if (method === 'wallet') {
      const bal = this.walletBalance();
      if (bal === null) { toast.error('Wallet balance not loaded yet.'); return; }
      if (bal <= 0) { toast.error('Wallet balance is ₹0.'); return; }

      if (this.walletCoversAll) {
        // Full wallet — place order directly
        this.placeOrder(null);
      } else {
        // Partial wallet — open Razorpay for the remainder
        this.openRazorpayModal(this.remainingAfterWallet);
      }
    } else {
      // Pure Razorpay
      this.openRazorpayModal(this.total);
    }
  }

  // ── Place order (called after payment is confirmed) ───────────────────────
  private placeOrder(rzpResponse: any): void {
    const addr = this.selectedAddress()!;
    this.isProcessing.set(true);
    const toastId = toast.loading('Placing order...');

    if (rzpResponse) console.log('Razorpay Payment Response:', rzpResponse);

    if (this.paymentMode() === 'cart') {
      const p = new PaginationModel(); p.pageSize = 1; p.pageNumber = 1;
      this.cartService.GetUserCart(p).subscribe({
        next: (cartRes) => {
          const req = new OrderAllFromCartRequestDTO();
          req.cartId = cartRes.data?.cartId ?? '';
          req.addressId = addr.addressId;
          req.paymentType = this.resolvedPaymentType;
          req.promoCode = this.appliedPromo() ?? '';
          req.useWallet = this.selectedPaymentMethod() === 'wallet';
          this.cartService.orderAllFromCart(req).subscribe({
            next: (res) => { this.isProcessing.set(false); toast.dismiss(toastId); if (res.data?.isSuccess) { toast.success('Order placed successfully!'); this.router.navigate(['/profile/orders']); } else toast.error(res.message || 'Failed to place order'); },
            error: (err) => { this.isProcessing.set(false); toast.dismiss(toastId); toast.error(err?.error?.message || 'Failed to place order'); },
          });
        },
        error: () => { this.isProcessing.set(false); toast.dismiss(toastId); toast.error('Failed to retrieve cart'); },
      });
    } else {
      const product = this.product();
      if (!product) { this.isProcessing.set(false); toast.dismiss(toastId); return; }
      const req: PlaceOrderRequestDTO = {
        addressId: addr.addressId,
        totalProductsCount: this.quantity(),
        totalAmount: this.subtotal,
        paymentType: this.resolvedPaymentType,
        promoCode: this.appliedPromo() ?? '',
        useWallet: this.selectedPaymentMethod() === 'wallet',
        orderProductdDetails: { productId: product.productId, productName: product.productName, quantity: this.quantity(), productPrice: product.price },
      };
      this.orderService.placeOrder(req).subscribe({
        next: (res) => { this.isProcessing.set(false); toast.dismiss(toastId); if (res.data?.isSuccess) { toast.success('Order placed successfully!'); this.router.navigate(['/profile/orders']); } else toast.error(res.message || 'Failed to place order'); },
        error: (err) => { this.isProcessing.set(false); toast.dismiss(toastId); toast.error(err?.error?.message || 'Failed to place order'); },
      });
    }
  }
}

// openRazorpayModal(amount: number): void {
//   const options = {
//     key: 'rzp_test_YOUR_KEY_HERE',
//     amount: Math.round(amount * 100), // paise
//     currency: 'INR',
//     name: 'Demo App',
//     description: 'Test Payment',
//     prefill: {
//       name: this.fullName(),
//       email: this.email(),
//       contact: this.phone(),
//     },
//     handler: (response: any) => {
//       console.log('Razorpay Payment Response:', response);
//       this.placeOrder(response); // existing logic untouched
//     },
//     modal: {
//       ondismiss: () => {
//         this.isProcessing.set(false);
//         toast.error('Payment Cancelled');
//       },
//     },
//   };
//   const rzp = new Razorpay(options);
//   rzp.open();
// }
