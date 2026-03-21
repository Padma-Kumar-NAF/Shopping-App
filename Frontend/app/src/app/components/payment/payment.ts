import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { ProductItem } from '../../models/product.model';
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
    const productId = this.route.snapshot.queryParamMap.get('productId');
    const qty = this.route.snapshot.queryParamMap.get('quantity');
    const fromCart = this.route.snapshot.queryParamMap.get('fromCart');

    // Check if coming from cart
    if (fromCart === 'true') {
      this.paymentMode.set('cart');
      this.loadCartItems();
    } else if (productId) {
      // Single product purchase
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
    // Mock cart items - replace with actual cart service call
    // You can integrate with your backend cart service here
    const mockCartItems: CartItem[] = [
      {
        id: '1',
        name: 'Wireless Bluetooth Headphones',
        price: 2999,
        quantity: 1,
        image: 'https://picsum.photos/400/300?random=1',
      },
      {
        id: '2',
        name: 'Smart Watch Pro',
        price: 8999,
        quantity: 2,
        image: 'https://picsum.photos/400/300?random=2',
      },
    ];
    this.cartItems.set(mockCartItems);
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
      this.router.navigate(['/product', this.product()?.id]);
    }
  }
}
