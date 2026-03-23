import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { toast } from 'ngx-sonner';

interface CartItem {
  id: number;
  name: string;
  price: number;
  quantity: number;
  image: string;
}

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class CheckoutComponent implements OnInit {
  private router = inject(Router);
  private cartService = inject(CartService);

  cartItems = signal<CartItem[]>([]);
  selectedPaymentMethod = signal<string>('card');
  isProcessing = signal<boolean>(false);

  // Payment form data
  cardNumber = signal<string>('');
  cardName = signal<string>('');
  expiryDate = signal<string>('');
  cvv = signal<string>('');
  upiId = signal<string>('');

  ngOnInit(): void {
    // Mock cart items - replace with actual cart data
    this.cartItems.set([
      {
        id: 1,
        name: 'Red Nail Polish',
        price: 799,
        quantity: 1,
        image: 'https://cdn.dummyjson.com/product-images/beauty/red-nail-polish/1.webp',
      },
      {
        id: 2,
        name: 'Nike Shoes',
        price: 4999,
        quantity: 2,
        image: 'https://cdn.dummyjson.com/product-images/fragrances/chanel-coco-noir-eau-de/1.webp',
      },
    ]);
  }

  get subtotal(): number {
    return this.cartItems().reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  get tax(): number {
    return this.subtotal * 0.18; // 18% tax
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

  processPayment(): void {
    const method = this.selectedPaymentMethod();

    // Validate payment details
    if (method === 'card') {
      if (!this.cardNumber() || !this.cardName() || !this.expiryDate() || !this.cvv()) {
        toast.error('Please fill all card details');
        return;
      }
    } else if (method === 'upi') {
      if (!this.upiId()) {
        toast.error('Please enter UPI ID');
        return;
      }
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
    this.router.navigate(['/profile/cart']);
  }
}
