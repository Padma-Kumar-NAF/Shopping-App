import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { AuthStateService } from '../../services/auth-state.service';
import { RedirectService } from '../../services/redirect.service';
import { CartService } from '../../services/cart.service';
import { WishlistService, WishListDTO } from '../../services/wishlist.service';
import { ProductDetails } from '../../models/users/product.model';
import { AddToCartRequestDTO } from '../../models/users/cart.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
  private productService = inject(ProductService);
  private productStateService = inject(ProductStateService);
  private router = inject(Router);
  private authState = inject(AuthStateService);
  private redirectService = inject(RedirectService);
  private cartService = inject(CartService);
  private wishlistService = inject(WishlistService);

  product = signal<ProductDetails | null>(null);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  quantity = signal<number>(1);
  selectedImage = signal<string>('');
  relatedProducts = signal<ProductDetails[]>([]);

  showWishlistPopup = signal<boolean>(false);
  wishlists = signal<WishListDTO[]>([]);

  ngOnInit(): void {
    const selectedProduct = this.productStateService.getSelectedProduct();
    if (selectedProduct) {
      this.loadProduct(selectedProduct);
    } else {
      toast.error('No product selected');
      this.router.navigate(['/products']);
    }
  }

  loadProduct(product: ProductDetails): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.product.set(product);
    this.selectedImage.set(product.imagePath);
    this.loadRelatedProducts(product.categoryName);
    this.isLoading.set(false);
  }

  loadRelatedProducts(categoryName: string): void {
    this.productService.getProductsByCategory(categoryName).subscribe({
      next: (products) => {
        this.relatedProducts.set(
          products.filter((p) => p.productId !== this.product()?.productId).slice(0, 4)
        );
      },
    });
  }

  increaseQuantity(): void {
    const maxStock = this.product()?.quantity ?? 10;
    if (this.quantity() < maxStock) {
      this.quantity.update((q) => q + 1);
    } else {
      toast.warning(`Maximum stock available: ${maxStock}`);
    }
  }

  decreaseQuantity(): void {
    if (this.quantity() > 1) this.quantity.update((q) => q - 1);
  }

  buyNow(): void {
    const product = this.product();
    if (!product) return;
    if (!this.authState.isAuthenticated()) {
      this.productStateService.setSelectedProduct(product);
      this.redirectService.storeIntendedRoute('/payment', { fromProduct: 'true', quantity: this.quantity() });
      toast.info('Please login to continue with your purchase');
      this.router.navigate(['/auth']);
      return;
    }
    this.productStateService.setSelectedProduct(product);
    this.router.navigate(['/payment'], { queryParams: { fromProduct: 'true', quantity: this.quantity() } });
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;
    if (!this.authState.isAuthenticated()) {
      toast.info('Please login to add items to cart');
      this.productStateService.setSelectedProduct(product);
      this.redirectService.storeIntendedRoute('/product-detail');
      this.router.navigate(['/auth']);
      return;
    }
    const request = new AddToCartRequestDTO();
    request.productId = product.productId;
    request.quantity = this.quantity();
    this.cartService.addToCart(request).subscribe({
      next: (res) => toast.success(res.message || `${product.productName} added to cart!`),
      error: (err) => toast.error(err?.error?.message || 'Failed to add to cart'),
    });
  }

  openWishlistPopup(): void {
    if (!this.authState.isAuthenticated()) {
      const product = this.product();
      if (product) this.productStateService.setSelectedProduct(product);
      toast.info('Please login to add items to wishlist');
      this.redirectService.storeIntendedRoute('/product-detail');
      this.router.navigate(['/auth']);
      return;
    }
    this.wishlistService.getUserWishlists().subscribe({
      next: (res) => this.wishlists.set(res.data?.wishList ?? []),
      error: () => this.wishlists.set([]),
    });
    this.showWishlistPopup.set(true);
  }

  addToWishlist(wishListId: string): void {
    const product = this.product();
    if (!product) return;
    this.wishlistService.addProduct(wishListId, product.productId).subscribe({
      next: (res) => {
        if (res.data?.isSuccess) {
          toast.success(`${product.productName} added to wishlist!`);
          this.showWishlistPopup.set(false);
        } else {
          toast.error(res.message || 'Failed to add to wishlist');
        }
      },
      error: (err) => toast.error(err?.error?.message || 'Failed to add to wishlist'),
    });
  }

  closeWishlistPopup(): void {
    this.showWishlistPopup.set(false);
  }

  viewRelatedProduct(product: ProductDetails): void {
    this.productStateService.setSelectedProduct(product);
    this.router.navigate(['/product-detail']).then(() => this.loadProduct(product));
  }

  getAverageRating(product: ProductDetails): number | null {
    if (!product.review?.length) return null;
    const avg = product.review.reduce((s, r) => s + r.reviewPoints, 0) / product.review.length;
    return Math.round(avg * 10) / 10;
  }

  getStarArray(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => (i < Math.floor(rating) ? 1 : 0));
  }
}
