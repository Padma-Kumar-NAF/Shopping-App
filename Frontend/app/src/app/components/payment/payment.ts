import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
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

  // ── Wallet ────────────────────────────────────────────────────────────────
  walletBalance = signal<number | null>(null);
  isLoadingWallet = signal<boolean>(false);
  /** Whether the user has opted to use wallet (checkbox toggle) */
  useWallet = signal<boolean>(false);

  /**
   * How much of the wallet will actually be applied.
   * = min(walletBalance, total)
   */
  get walletApplied(): number {
    const bal = this.walletBalance();
    if (!this.useWallet() || bal === null) return 0;
    return Math.min(bal, this.total);
  }

  /** Amount still owed after wallet deduction */
  get remainingAfterWallet(): number {
    return Math.max(0, this.total - this.walletApplied);
  }

  /** True when wallet alone covers the full order */
  get walletCoversAll(): boolean {
    const bal = this.walletBalance();
    return this.useWallet() && bal !== null && bal >= this.total;
  }

  // ── Payment mode ──────────────────────────────────────────────────────────
  paymentMode = signal<'single' | 'cart'>('single');

  // Single product purchase
  product = signal<ProductDetails | SearchProductByIdResponseDTO | null>(null);
  quantity = signal<number>(1);

  // Cart purchase
  cartItems = signal<CartItem[]>([]);

  /**
   * The secondary (non-wallet) payment method.
   * Only required when wallet doesn't cover the full amount.
   */
  selectedPaymentMethod = signal<string>('card');
  isProcessing = signal<boolean>(false);

  // Payment form data
  cardNumber = signal<string>('');
  cardName = signal<string>('');
  expiryDate = signal<string>('');
  cvv = signal<string>('');
  upiId = signal<string>('');

  // Shipping details
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

  // Promo code
  promoInput = signal<string>('');
  appliedPromo = signal<string | null>(null);
  discountPercent = signal<number>(0);
  promoError = signal<string | null>(null);
  isValidatingPromo = signal<boolean>(false);

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

  // ── Address helpers ───────────────────────────────────────────────────────

  private loadAddresses(): void {
    const cached = this.addressSelectionService.getAvailableAddresses();
    if (cached.length === 0) {
      const pagination = new PaginationModel();
      pagination.pageSize = 10;
      pagination.pageNumber = 1;
      this.addressApiService.GetUserAddresses(pagination).subscribe({
        next: (response) => {
          if (response?.data?.addressList) {
            this.addressSelectionService.setAvailableAddresses(response.data.addressList);
          }
        },
        error: () => {},
      });
    }
    const selected = this.addressSelectionService.getSelectedAddress();
    if (selected) this.applyAddress(selected);
  }

  selectAddress(addr: AddressDTO): void {
    this.addressSelectionService.setSelectedAddress(addr);
    this.applyAddress(addr);
    this.showAddressPicker.set(false);
  }

  unselectAddress(): void {
    this.selectedAddress.set(null);
    this.addressSelectionService.clearSelectedAddress();
    this.address.set('');
    this.city.set('');
    this.state.set('');
    this.pincode.set('');
  }

  private applyAddress(addr: AddressDTO): void {
    this.selectedAddress.set(addr);
    this.address.set(`${addr.addressLine1}${addr.addressLine2 ? ', ' + addr.addressLine2 : ''}`);
    this.city.set(addr.city);
    this.state.set(addr.state);
    this.pincode.set(addr.pincode);
  }

  // ── Product / cart loaders ────────────────────────────────────────────────

  private loadProductFromState(): void {
    const product = this.productStateService.getSelectedProduct();
    if (product) {
      this.product.set(product);
      return;
    }
    // State is gone (e.g. post-login redirect) — recover via productId in the URL
    const productId = this.route.snapshot.queryParamMap.get('productId');
    if (productId) {
      this.loadSingleProduct(productId);
    } else {
      toast.error('Product not found');
      this.router.navigate(['/products']);
    }
  }

  private loadSingleProduct(productId: string): void {
    this.productService.getProductById(productId).subscribe({
      next: (product) => {
        if (product) {
          this.product.set(product);
        } else {
          toast.error('Product not found');
          this.router.navigate(['/products']);
        }
      },
      error: () => {
        toast.error('Failed to load product');
        this.router.navigate(['/products']);
      },
    });
  }

  private loadCartItems(): void {
    const pagination = new PaginationModel();
    pagination.pageSize = 100;
    pagination.pageNumber = 1;
    this.cartService.GetUserCart(pagination).subscribe({
      next: (response) => {
        if (response.data?.cartItems) {
          const items: CartItem[] = response.data.cartItems.map((item) => ({
            id: item.productId,
            name: item.productName,
            price: item.price,
            quantity: item.quantity,
            image: item.imagePath,
          }));
          this.cartItems.set(items);
        }
      },
      error: (err) => {
        console.error('Failed to load cart items:', err);
        toast.error('Failed to load cart items');
        this.router.navigate(['/profile/cart']);
      },
    });
  }

  // ── Order totals ──────────────────────────────────────────────────────────

  get subtotal(): number {
    if (this.paymentMode() === 'cart') {
      return this.cartItems().reduce((sum, item) => sum + item.price * item.quantity, 0);
    }
    return (this.product()?.price || 0) * this.quantity();
  }

  get totalItems(): number {
    if (this.paymentMode() === 'cart') {
      return this.cartItems().reduce((sum, item) => sum + item.quantity, 0);
    }
    return this.quantity();
  }

  get tax(): number { return this.subtotal * 0.18; }
  get shipping(): number { return this.subtotal > 5000 ? 0 : 99; }
  get discountAmount(): number { return (this.subtotal * this.discountPercent()) / 100; }
  get total(): number { return this.subtotal - this.discountAmount + this.tax + this.shipping; }

  // ── Promo code ────────────────────────────────────────────────────────────

  applyPromo(): void {
    if (this.appliedPromo() || this.isValidatingPromo()) return;
    const code = this.promoInput().trim().toUpperCase();
    if (!code) { this.promoError.set('Please enter a promo code.'); return; }

    this.isValidatingPromo.set(true);
    this.promoError.set(null);

    this.promoCodeService.validatePromoCode({ promoCodeName: code }).subscribe({
      next: (res) => {
        this.isValidatingPromo.set(false);
        if (res.data?.isValid) {
          this.appliedPromo.set(code);
          this.discountPercent.set(res.data.discountPercentage);
          toast.success(`Promo code applied! ${res.data.discountPercentage}% off`);
        } else {
          this.promoError.set(res.message || 'Invalid promo code. Please try again.');
        }
      },
      error: (err) => {
        this.isValidatingPromo.set(false);
        this.promoError.set(err?.error?.message || 'Invalid promo code. Please try again.');
      },
    });
  }

  removePromo(): void {
    this.appliedPromo.set(null);
    this.discountPercent.set(0);
    this.promoInput.set('');
    this.promoError.set(null);
  }

  // ── Wallet toggle ─────────────────────────────────────────────────────────

  toggleWallet(): void {
    const next = !this.useWallet();
    this.useWallet.set(next);

    if (next && this.walletBalance() === null) {
      this.fetchWalletBalance();
    }

    // If wallet now covers everything, clear the secondary method selection
    if (!next) {
      this.walletBalance.set(null);
    }
  }

  private fetchWalletBalance(): void {
    this.isLoadingWallet.set(true);
    this.walletService.getWalletBalance().subscribe({
      next: (res: ApiResponse<GetWalletAmountResponseDTO>) => {
        const balance = res.data?.walletBalance ?? 0;
        this.walletBalance.set(balance);
        this.isLoadingWallet.set(false);

        if (balance >= this.total) {
          // Full coverage — no secondary method needed
          toast.success(`Wallet balance (₹${balance.toLocaleString()}) covers the full order.`);
        } else if (balance > 0) {
          // Partial coverage — inform user
          toast.info(`₹${balance.toLocaleString()} will be used from wallet. Select a method for the remaining ₹${(this.total - balance).toLocaleString()}.`);
        } else {
          toast.error('Your wallet balance is ₹0. Please choose another payment method.');
          this.useWallet.set(false);
          this.walletBalance.set(null);
        }
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Could not fetch wallet balance.');
        this.isLoadingWallet.set(false);
        this.useWallet.set(false);
        this.walletBalance.set(null);
      },
    });
  }

  // ── Secondary payment method ──────────────────────────────────────────────

  selectPaymentMethod(method: string): void {
    // Wallet-only mode: secondary method selection is disabled
    if (this.walletCoversAll) return;
    this.selectedPaymentMethod.set(method);
  }

  // ── Quantity ──────────────────────────────────────────────────────────────

  updateQuantity(change: number): void {
    const newQty = this.quantity() + change;
    if (newQty >= 1 && newQty <= (this.product()?.quantity || 1)) {
      this.quantity.set(newQty);
    }
  }

  // ── Validation ────────────────────────────────────────────────────────────

  validateShippingDetails(): boolean {
    if (!this.fullName() || !this.email() || !this.phone() ||
        !this.address() || !this.city() || !this.state() || !this.pincode()) {
      toast.error('Please fill all shipping details');
      return false;
    }
    return true;
  }

  validatePaymentDetails(): boolean {
    // Must have at least one payment source
    if (!this.useWallet() && !this.selectedPaymentMethod()) {
      toast.error('Please select a payment method');
      return false;
    }

    // Wallet checks
    if (this.useWallet()) {
      const balance = this.walletBalance();
      if (balance === null) {
        toast.error('Wallet balance not loaded yet. Please wait.');
        return false;
      }
      if (balance <= 0) {
        toast.error('Wallet balance is ₹0. Please choose another payment method.');
        return false;
      }
    }

    // If wallet doesn't cover everything, validate the secondary method
    if (!this.walletCoversAll) {
      const method = this.selectedPaymentMethod();
      if (method === 'card') {
        if (!this.cardNumber() || !this.cardName() || !this.expiryDate() || !this.cvv()) {
          toast.error('Please fill all card details');
          return false;
        }
      } else if (method === 'upi') {
        if (!this.upiId()) {
          toast.error('Please enter your UPI ID');
          return false;
        }
      }
      // cod requires no extra input
    }

    return true;
  }

  /**
   * Builds the paymentType string sent to the backend:
   * - "wallet"           → full wallet payment
   * - "wallet+card"      → partial wallet + card
   * - "wallet+upi"       → partial wallet + UPI
   * - "wallet+cod"       → partial wallet + COD
   * - "card" / "upi" / "cod" → no wallet involved
   */
  get resolvedPaymentType(): string {
    if (this.useWallet() && this.walletCoversAll) return 'wallet';
    if (this.useWallet() && !this.walletCoversAll) return `wallet+${this.selectedPaymentMethod()}`;
    return this.selectedPaymentMethod();
  }

  // ── Place order ───────────────────────────────────────────────────────────

  processPayment(): void {
    if (!this.validateShippingDetails() || !this.validatePaymentDetails()) return;

    const addr = this.selectedAddress();
    if (!addr) {
      toast.error('Please select a delivery address');
      return;
    }

    this.isProcessing.set(true);
    const toastId = toast.loading('Processing payment...');

    if (this.paymentMode() === 'cart') {
      const pagination = new PaginationModel();
      pagination.pageSize = 1;
      pagination.pageNumber = 1;
      this.cartService.GetUserCart(pagination).subscribe({
        next: (cartRes) => {
          const request = new OrderAllFromCartRequestDTO();
          request.cartId = cartRes.data?.cartId ?? '';
          request.addressId = addr.addressId;
          request.paymentType = this.resolvedPaymentType;
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
        },
        error: () => {
          this.isProcessing.set(false);
          toast.dismiss(toastId);
          toast.error('Failed to retrieve cart');
        },
      });
    } else {
      const product = this.product();
      if (!product) { this.isProcessing.set(false); toast.dismiss(toastId); return; }

      const request: PlaceOrderRequestDTO = {
        addressId: addr.addressId,
        totalProductsCount: this.quantity(),
        totalAmount: this.total,
        paymentType: this.resolvedPaymentType,
        promoCode: this.appliedPromo() ?? '',
        orderProductdDetails: {
          productId: product.productId,
          productName: product.productName,
          quantity: this.quantity(),
          productPrice: product.price,
        },
      };

      this.orderService.placeOrder(request).subscribe({
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
}
