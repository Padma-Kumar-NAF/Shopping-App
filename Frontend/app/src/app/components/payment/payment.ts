import { Component, OnInit, inject, signal } from '@angular/core';
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
import { AddressDTO } from '../../models/users/address.model';
import { PaginationModel } from '../../models/users/pagination.model';
import { ProductDetails, SearchProductByIdResponseDTO } from '../../models/users/product.model';
import { OrderAllFromCartRequestDTO } from '../../models/users/cart.model';
import { toast } from 'ngx-sonner';

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

  // Payment mode: 'single' for single product, 'cart' for cart items
  paymentMode = signal<'single' | 'cart'>('single');

  // Single product purchase
  product = signal<ProductDetails | SearchProductByIdResponseDTO | null>(null);
  quantity = signal<number>(1);

  // Cart purchase
  cartItems = signal<CartItem[]>([]);

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

  // Address selection
  availableAddresses = signal<AddressDTO[]>([]);
  selectedAddress = signal<AddressDTO | null>(null);
  showAddressPicker = signal<boolean>(false);

  ngOnInit(): void {
    const fromCart = this.route.snapshot.queryParamMap.get('fromCart');
    const fromProduct = this.route.snapshot.queryParamMap.get('fromProduct');
    const qty = this.route.snapshot.queryParamMap.get('quantity');

    // Check if coming from cart
    if (fromCart === 'true') {
      this.paymentMode.set('cart');
      this.loadCartItems();
    } else if (fromProduct === 'true') {
      // Single product purchase - get from state
      this.paymentMode.set('single');
      if (qty) {
        this.quantity.set(parseInt(qty));
      }
      this.loadProductFromState();
    } else {
      // Fallback: check for productId in query params (for backward compatibility)
      const productId = this.route.snapshot.queryParamMap.get('productId');
      if (productId) {
        this.paymentMode.set('single');
        if (qty) {
          this.quantity.set(parseInt(qty));
        }
        this.loadSingleProduct(productId);
      } else {
        toast.error('No items to checkout');
        this.router.navigate(['/products']);
      }
    }

    this.loadAddresses();

    // Pre-fill name and email from auth store
    this.fullName.set(this.authStateService.username());
    this.email.set(this.authStateService.email());
  }

  private loadAddresses(): void {
    // Use addresses already loaded in the service (from home page)
    const cached = this.addressSelectionService.getAvailableAddresses();
    if (cached.length > 0) {
      this.availableAddresses.set(cached);
    } else {
      const pagination = new PaginationModel();
      pagination.pageSize = 10;
      pagination.pageNumber = 1;
      this.addressApiService.GetUserAddresses(pagination).subscribe({
        next: (response) => {
          if (response?.data?.addressList) {
            this.availableAddresses.set(response.data.addressList);
            this.addressSelectionService.setAvailableAddresses(response.data.addressList);
          }
        },
        error: () => {},
      });
    }

    // Pre-fill from previously selected address
    const selected = this.addressSelectionService.getSelectedAddress();
    if (selected) {
      this.applyAddress(selected);
    }
  }

  selectAddress(addr: AddressDTO): void {
    this.selectedAddress.set(addr);
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

  private loadProductFromState(): void {
    const product = this.productStateService.getSelectedProduct();
    if (product) {
      this.product.set(product);
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
    // Load actual cart items from the cart service
    const pagination = new PaginationModel();
    pagination.pageSize = 100; // Load all items for checkout
    pagination.pageNumber = 1;

    this.cartService.GetUserCart(pagination).subscribe({
      next: (response) => {
        if (response.data?.cartItems) {
          // Map backend CartItemDTO to local CartItem interface
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

  get tax(): number {
    return this.subtotal * 0.18; // 18% GST
  }

  get shipping(): number {
    return this.subtotal > 5000 ? 0 : 99;
  }

  get total(): number {
    return this.subtotal + this.tax + this.shipping;
  }

  selectPaymentMethod(method: string): void {
    this.selectedPaymentMethod.set(method);
  }

  updateQuantity(change: number): void {
    const newQty = this.quantity() + change;
    if (newQty >= 1 && newQty <= (this.product()?.quantity || 1)) {
      this.quantity.set(newQty);
    }
  }

  validateShippingDetails(): boolean {
    if (
      !this.fullName() ||
      !this.email() ||
      !this.phone() ||
      !this.address() ||
      !this.city() ||
      !this.state() ||
      !this.pincode()
    ) {
      toast.error('Please fill all shipping details');
      return false;
    }
    return true;
  }

  validatePaymentDetails(): boolean {
    const method = this.selectedPaymentMethod();

    if (method === 'card') {
      if (!this.cardNumber() || !this.cardName() || !this.expiryDate() || !this.cvv()) {
        toast.error('Please fill all card details');
        return false;
      }
    } else if (method === 'upi') {
      if (!this.upiId()) {
        toast.error('Please enter UPI ID');
        return false;
      }
    }
    return true;
  }

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
      // Order all items from cart
      const cartId = this.cartItems().length > 0 ? (this.cartItems() as any)[0]?.cartId : '';
      // We need the cartId — reload cart to get it
      const pagination = new PaginationModel();
      pagination.pageSize = 1;
      pagination.pageNumber = 1;
      this.cartService.GetUserCart(pagination).subscribe({
        next: (cartRes) => {
          const request = new OrderAllFromCartRequestDTO();
          request.cartId = cartRes.data?.cartId ?? '';
          request.addressId = addr.addressId;
          request.paymentType = this.selectedPaymentMethod();
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
      // Single product order
      const product = this.product();
      if (!product) { this.isProcessing.set(false); toast.dismiss(toastId); return; }
      const request: PlaceOrderRequestDTO = {
        addressId: addr.addressId,
        totalProductsCount: this.quantity(),
        totalAmount: this.total,
        paymentType: this.selectedPaymentMethod(),
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
