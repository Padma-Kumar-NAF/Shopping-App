import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product';
import { ProductItem } from '../../models/product.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  product = signal<ProductItem | null>(null);
  isLoading = signal<boolean>(true);
  quantity = signal<number>(1);
  selectedImage = signal<string>('');
  relatedProducts = signal<ProductItem[]>([]);

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

  addToCart(): void {
    const product = this.product();
    if (product) {
      console.log('Adding to cart:', {
        product,
        quantity: this.quantity(),
      });
      toast.success(`${product.name} added to cart!`);
    }
  }

  buyNow(): void {
    const product = this.product();
    if (product) {
      console.log('Buy now:', {
        product,
        quantity: this.quantity(),
      });
      toast.success('Proceeding to checkout...');
      // Navigate to checkout
    }
  }

  addToWishlist(): void {
    const product = this.product();
    if (product) {
      console.log('Adding to wishlist:', product);
      toast.success(`${product.name} added to wishlist!`);
    }
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
