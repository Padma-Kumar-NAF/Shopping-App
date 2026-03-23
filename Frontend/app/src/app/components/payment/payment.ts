import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { CartService } from '../../services/cart.service';
import { ProductItem } from '../../models/users/product.model';
import { PaginationModel } from '../../models/users/pagination.model';
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

  // Payment mode: 'single' for single product, 'cart' for cart items
  paymentMode = signal<'single' | 'cart'>('single');

  // Single product purchase
  product = signal<ProductItem | null>(null);
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
    if (newQty >= 1 && newQty <= (this.product()?.stock || 1)) {
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
    if (!this.validateShippingDetails() || !this.validatePaymentDetails()) {
      return;
    }

    this.isProcessing.set(true);
    const toastId = toast.loading('Processing payment...');

    // Simulate payment processing
    setTimeout(() => {
      this.isProcessing.set(false);
      toast.dismiss(toastId);
      toast.success('Payment successful! Order placed.');

      // Navigate to orders page
      this.router.navigate(['/profile/orders']);
    }, 2000);
  }

  goBack(): void {
    if (this.paymentMode() === 'cart') {
      this.router.navigate(['/profile/cart']);
    } else {
      const product = this.product();
      if (product) {
        this.productStateService.setSelectedProduct(product);
      }
      this.router.navigate(['/product-detail']);
    }
  }
}
