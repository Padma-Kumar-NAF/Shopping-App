import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { AuthStateService } from '../../services/auth-state.service';
import { RedirectService } from '../../services/redirect.service';
import { ProductItem } from '../../models/users/product.model';
import { toast } from 'ngx-sonner';

interface WishlistItem {
  id: number;
  name: string;
  products: { id: string; name: string; price: number; image: string }[];
}

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authState = inject(AuthStateService);
  private redirectService = inject(RedirectService);

  product = signal<ProductItem | null>(null);
  isLoading = signal<boolean>(true);
  quantity = signal<number>(1);
  selectedImage = signal<string>('');
  relatedProducts = signal<ProductItem[]>([]);

  // Wishlist popup state
  showWishlistPopup = signal<boolean>(false);
  wishlists = signal<WishlistItem[]>([
    {
      id: 1,
      name: 'Dream Wardrobe',
      products: [
        {
          id: '101',
          name: 'Premium Leather Jacket',
          price: 4999,
          image: 'https://images.unsplash.com/photo-1551028719-00167b16eac5?w=400&h=400&fit=crop',
        },
        {
          id: '102',
          name: 'Designer High-Top Sneakers',
          price: 2999,
          image: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=400&fit=crop',
        },
      ],
    },
    {
      id: 2,
      name: 'Tech Gadgets',
      products: [
        {
          id: '201',
          name: 'Wireless Headphones',
          price: 12999,
          image:
            'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&h=400&fit=crop',
        },
      ],
    },
  ]);

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const productId = params['id'];
      if (productId) {
        this.loadProduct(productId);
      }
    });
  }

  loadProduct(id: string): void {
    this.isLoading.set(true);
    this.productService.getProductById(id).subscribe({
      next: (product) => {
        if (product) {
          this.product.set(product);
          this.selectedImage.set(product.image);
          this.loadRelatedProducts(product.category);
        } else {
          toast.error('Product not found');
          this.router.navigate(['/products']);
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading product:', error);
        toast.error('Failed to load product');
        this.isLoading.set(false);
      },
    });
  }

  loadRelatedProducts(category: string): void {
    this.productService.getProductsByCategory(category).subscribe({
      next: (products) => {
        // Exclude current product and limit to 4
        const filtered = products.filter((p) => p.id !== this.product()?.id).slice(0, 4);
        this.relatedProducts.set(filtered);
      },
    });
  }

  increaseQuantity(): void {
    const currentQty = this.quantity();
    const maxStock = this.product()?.stock || 10;
    if (currentQty < maxStock) {
      this.quantity.update((q) => q + 1);
    } else {
      toast.warning(`Maximum stock available: ${maxStock}`);
    }
  }

  decreaseQuantity(): void {
    if (this.quantity() > 1) {
      this.quantity.update((q) => q - 1);
    }
  }

  buyNow(): void {
    const product = this.product();
    if (!product) return;

    // Check if user is authenticated
    if (!this.authState.isAuthenticated()) {
      // Store the intended action
      this.redirectService.storeIntendedRoute('/payment', {
        productId: product.id,
        quantity: this.quantity(),
      });

      toast.info('Please login to continue with your purchase');
      this.router.navigate(['/auth']);
      return;
    }

    // User is authenticated, proceed to payment
    this.router.navigate(['/payment'], {
      queryParams: {
        productId: product.id,
        quantity: this.quantity(),
      },
    });
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;

    // Check if user is authenticated
    if (!this.authState.isAuthenticated()) {
      toast.info('Please login to add items to cart');
      this.redirectService.storeIntendedRoute(`/product/${product.id}`);
      this.router.navigate(['/auth']);
      return;
    }

    // User is authenticated, add to cart
    console.log('Adding to cart:', {
      product,
      quantity: this.quantity(),
    });
    toast.success(`${product.name} added to cart!`);
  }

  openWishlistPopup(): void {
    // Check if user is authenticated
    if (!this.authState.isAuthenticated()) {
      const product = this.product();
      toast.info('Please login to add items to wishlist');
      this.redirectService.storeIntendedRoute(`/product/${product?.id}`);
      this.router.navigate(['/auth']);
      return;
    }

    this.showWishlistPopup.set(true);
  }

  addToWishlist(wishlistId: number): void {
    const product = this.product();
    if (!product) return;

    const alreadyAdded = this.wishlists().some(
      (w) => w.id === wishlistId && w.products.some((p) => p.id === product.id)
    );

    if (alreadyAdded) {
      toast.warning(`${product.name} is already in this wishlist!`);
      return;
    }

    this.wishlists.update((lists) =>
      lists.map((w) =>
        w.id === wishlistId
          ? {
              ...w,
              products: [
                ...w.products,
                { id: product.id, name: product.name, price: product.price, image: product.image },
              ],
            }
          : w
      )
    );
    toast.success(`${product.name} added to wishlist!`);
    this.showWishlistPopup.set(false);
  }

  closeWishlistPopup(): void {
    this.showWishlistPopup.set(false);
  }

  goBack(): void {
    this.router.navigate(['/products']);
  }

  viewRelatedProduct(productId: string): void {
    this.router.navigate(['/product', productId]);
  }

  getStarArray(rating: number): number[] {
    return Array(5)
      .fill(0)
      .map((_, i) => (i < Math.floor(rating) ? 1 : 0));
  }
}
