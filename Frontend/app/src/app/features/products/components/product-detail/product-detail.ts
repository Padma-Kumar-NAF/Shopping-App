import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../../../core/state/product-state.service';
import { AuthStateService } from '../../../../core/state/auth-state.service';
import { RedirectService } from '../../../../core/services/redirect.service';
import { CartService } from '../../../../features/user/services/cart.service';
import { WishlistService, WishListDTO } from '../../../../features/user/services/wishlist.service';
import { ProductDetails } from '../../../../shared/models/users/product.model';
import { AddToCartRequestDTO } from '../../../../shared/models/users/cart.model';
import { toast } from 'ngx-sonner';
import { PaginationModel } from '../../../../shared/models/users/pagination.model';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit, OnDestroy {
  private productService = inject(ProductService);
  private productStateService = inject(ProductStateService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authState = inject(AuthStateService);
  private redirectService = inject(RedirectService);
  private cartService = inject(CartService);
  private wishlistService = inject(WishlistService);

  private destroy$ = new Subject<void>();
  wishListpagination : PaginationModel;

  paginaton: PaginationModel;
  constructor() {
    this.paginaton = new PaginationModel();
    this.paginaton.pageNumber = 1;
    this.paginaton.pageSize = 10;

    this.wishListpagination = new PaginationModel();
    this.wishListpagination.pageNumber = 1;
    this.wishListpagination.pageSize = 20;
  }

  product = signal<ProductDetails | null>(null);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  quantity = signal<number>(1);
  selectedImage = signal<string>('');
  relatedProducts = signal<ProductDetails[]>([]);

  showWishlistPopup = signal<boolean>(false);
  wishlists = signal<WishListDTO[]>([]);

  ngOnInit(): void {
    window.scrollTo({ top: 0, behavior: 'instant' });
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const productId = params.get('productId');
      window.scrollTo({ top: 0, behavior: 'instant' });
      this.resolveProduct(productId);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private resolveProduct(productId: string | null): void {
    const stateProduct = this.productStateService.getSelectedProduct();

    if (stateProduct && stateProduct.productId === productId) {
      this.loadProduct(stateProduct);
    } else if (productId) {
      this.isLoading.set(true);
      this.productService.getProductById(productId).subscribe({
        next: (product) => {
          if (product) {
            this.productStateService.setSelectedProduct(product as any);
            this.loadProduct(product as any);
          } else {
            this.error.set('Product not found');
            this.isLoading.set(false);
          }
        },
        error: () => {
          this.error.set('Failed to load product');
          this.isLoading.set(false);
        },
      });
    } else {
      this.error.set('No product specified');
      this.isLoading.set(false);
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
    this.productService.getProductsByCategory(categoryName, this.paginaton).subscribe({
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
      this.redirectService.storeIntendedRoute(
        `/payment?fromProduct=true&quantity=${this.quantity()}&productId=${product.productId}`
      );
      toast.info('Please login to continue with your purchase');
      this.router.navigate(['/auth']);
      return;
    }
    this.productStateService.setSelectedProduct(product);
    this.router.navigate(['/payment'], {
      queryParams: { fromProduct: 'true', quantity: this.quantity(), productId: product.productId },
    });
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;
    if (!this.authState.isAuthenticated()) {
      toast.info('Please login to add items to cart');
      this.productStateService.setSelectedProduct(product);
      this.redirectService.storeIntendedRoute(`/product-detail/${product.productId}`);
      this.router.navigate(['/auth']);
      return;
    }
    const request = new AddToCartRequestDTO();
    request.productId = product.productId;
    request.quantity = this.quantity();
    this.cartService.addToCart(request).subscribe({
      next: (res) => {
        toast.success(res.message || `${product.productName} added to cart!`)
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to add to cart')
        console.error(err);
      }
    });
  }

  openWishlistPopup(): void {
    if (!this.authState.isAuthenticated()) {
      const product = this.product();
      if (product) this.productStateService.setSelectedProduct(product);
      toast.info('Please login to add items to wishlist');
      this.redirectService.storeIntendedRoute(`/product-detail/${this.product()?.productId ?? ''}`);
      this.router.navigate(['/auth']);
      return;
    }
    this.wishlistService.getUserWishlists(this.wishListpagination).subscribe({
      next: (res) => {
        this.wishlists.set(res.data?.wishList ?? [])
      },
      error: (err) => {
        console.error(err);
        this.wishlists.set([])
      },
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
    this.router.navigate(['/product-detail', product.productId]);
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
