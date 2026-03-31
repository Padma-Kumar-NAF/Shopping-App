import { Component, OnInit, inject, signal, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { environment } from '../../../environments/environment';

declare var Stripe: any;
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { CartService } from '../../services/userServices/cart.service';
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

  // ── Selected top-level method: 'stripe' | 'wallet' ──────────────────────
  selectedPaymentMethod = signal<'stripe' | 'wallet'>('stripe');
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

  // ── Stripe modal ──────────────────────────────────────────────────────────
  showStripeModal = signal<boolean>(false);
  stripeAmount = signal<number>(0);
  isStripeProcessing = signal<boolean>(false);
  stripeError = signal<string | null>(null);

  private stripe: any = null;
  private stripeCard: any = null;

  @ViewChild('stripeCardElement') stripeCardElementRef!: ElementRef;

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

  get tax(): number {
    return Math.round(this.subtotal * 0.18 * 100) / 100;
  }
  get shipping(): number {
    return this.subtotal > 5000 ? 0 : 100;
  }
  get discountAmount(): number {
    return Math.round(((this.subtotal * this.discountPercent()) / 100) * 100) / 100;
  }
  get total(): number {
    return this.subtotal - this.discountAmount + this.tax + this.shipping;
  }

  // ── resolvedPaymentType for backend ──────────────────────────────────────
  get resolvedPaymentType(): string {
    if (this.selectedPaymentMethod() === 'wallet') {
      if (this.walletCoversAll) return 'wallet';
      return 'wallet+stripe';
    }
    return 'stripe';
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

  removePromo(): void {
    this.appliedPromo.set(null);
    this.discountPercent.set(0);
    this.promoInput.set('');
    this.promoError.set(null);
  }

  // ── Wallet selection ──────────────────────────────────────────────────────
  selectMethod(method: 'stripe' | 'wallet'): void {
    this.selectedPaymentMethod.set(method);
    if (method === 'wallet' && this.walletBalance() === null) this.fetchWalletBalance();
    if (method === 'stripe') { this.walletBalance.set(null); }
  }

  private fetchWalletBalance(): void {
    this.isLoadingWallet.set(true);
    this.walletService.getWalletBalance().subscribe({
      next: (res: ApiResponse<GetWalletAmountResponseDTO>) => {
        const bal = res.data?.walletBalance ?? 0;
        this.walletBalance.set(bal);
        this.isLoadingWallet.set(false);
        if (bal <= 0) { toast.error('Wallet balance is ₹0.'); this.selectedPaymentMethod.set('stripe'); this.walletBalance.set(null); }
        else if (bal >= this.total) toast.success(`Wallet (₹${bal.toLocaleString()}) covers the full order.`);
        else toast.info(`₹${bal.toLocaleString()} from wallet. Stripe will handle the remaining ₹${(this.total - bal).toLocaleString()}.`);
      },
      error: (err) => { toast.error(err?.error?.message || 'Could not fetch wallet balance.'); this.isLoadingWallet.set(false); this.selectedPaymentMethod.set('stripe'); },
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

  // ── Stripe modal ──────────────────────────────────────────────────────────
  openStripeModal(amount: number): void {
    // console.log('Stripe Config:', {
    //   publishableKey: environment.stripe.publishableKey,
    //   appName: environment.stripe.appName,
    //   currency: environment.stripe.currency,
    //   amount,
    //   production: environment.production,
    // });
    this.stripeAmount.set(amount);
    this.stripeError.set(null);
    this.showStripeModal.set(true);
    setTimeout(() => {
      this.stripe = Stripe(environment.stripe.publishableKey);
      const elements = this.stripe.elements();
      this.stripeCard = elements.create('card', {
        style: {
          base: { fontSize: '16px', color: '#1a1a1a', fontFamily: 'Poppins, sans-serif', '::placeholder': { color: '#9ca3af' } },
          invalid: { color: '#ef4444' },
        },
      });
      this.stripeCard.mount('#stripe-card-element');
      this.stripeCard.on('change', (event: any) => {
        this.stripeError.set(event.error ? event.error.message : null);
      });
    }, 100);
  }

  closeStripeModal(): void {
    if (this.stripeCard) { this.stripeCard.destroy(); this.stripeCard = null; }
    this.showStripeModal.set(false);
    this.isStripeProcessing.set(false);
    this.isProcessing.set(false);
    toast.error('Payment Cancelled');
  }

  async submitStripePayment(): Promise<void> {
    if (!this.stripe || !this.stripeCard) return;
    this.isStripeProcessing.set(true);
    this.stripeError.set(null);
    const { paymentMethod, error } = await this.stripe.createPaymentMethod({
      type: 'card',
      card: this.stripeCard,
      billing_details: { name: this.fullName(), email: this.email() },
    });
    if (error) {
      this.stripeError.set(error.message);
      this.isStripeProcessing.set(false);
      return;
    }
    console.log('Stripe Payment Response:', paymentMethod);
    this.isStripeProcessing.set(false);
    if (this.stripeCard) { this.stripeCard.destroy(); this.stripeCard = null; }
    this.showStripeModal.set(false);
    this.placeOrder(paymentMethod);
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
        this.placeOrder(null);
      } else {
        // Partial wallet — open Stripe for the remainder
        this.openStripeModal(this.remainingAfterWallet);
      }
    } else {
      // Pure Stripe
      this.openStripeModal(this.total);
    }
  }

  // ── Place order (called after payment is confirmed) ───────────────────────
  private placeOrder(stripeResponse: any): void {
    const addr = this.selectedAddress()!;
    this.isProcessing.set(true);
    const toastId = toast.loading('Placing order...');

    if (stripeResponse) console.log('Stripe Payment Response:', stripeResponse);


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
          req.stripePaymentId = stripeResponse?.id ?? '';
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
        stripePaymentId: stripeResponse?.id ?? '',
        orderProductdDetails: { productId: product.productId, productName: product.productName, quantity: this.quantity(), productPrice: product.price },
      };
      this.orderService.placeOrder(req).subscribe({
        next: (res) => { this.isProcessing.set(false); toast.dismiss(toastId); if (res.data?.isSuccess) { toast.success('Order placed successfully!'); this.router.navigate(['/profile/orders']); } else toast.error(res.message || 'Failed to place order'); },
        error: (err) => { this.isProcessing.set(false); toast.dismiss(toastId); toast.error(err?.error?.message || 'Failed to place order'); },
      });
    }
  }
}
