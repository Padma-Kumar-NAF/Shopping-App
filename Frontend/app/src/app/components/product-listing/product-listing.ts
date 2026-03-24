import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../services/product-state.service';
import { AuthStateService } from '../../services/auth-state.service';
import { RedirectService } from '../../services/redirect.service';
import { ProductDetails } from '../../models/users/product.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-product-listing',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-listing.html',
  styleUrl: './product-listing.css',
})
export class ProductListing implements OnInit {
  private productService = inject(ProductService);
  private productStateService = inject(ProductStateService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authState = inject(AuthStateService);
  private redirectService = inject(RedirectService);

  searchQuery = signal<string>('');
  allProducts = signal<ProductDetails[]>([]);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);
  hasSearched = signal<boolean>(false);
  selectedCategory = signal<string>('all');
  categories = signal<string[]>([]);
  filteredProducts = signal<ProductDetails[]>([]);

  // Price filter signals
  minPrice = signal<number>(0);
  maxPrice = signal<number>(100000);
  priceRangeMin = signal<number>(0);
  priceRangeMax = signal<number>(100000);

  showFilters = signal<boolean>(false);

  priceRanges = [
    { label: 'All Prices', min: 0, max: 100000 },
    { label: 'Under ₹1,000', min: 0, max: 1000 },
    { label: '₹1,000 - ₹5,000', min: 1000, max: 5000 },
    { label: '₹5,000 - ₹10,000', min: 5000, max: 10000 },
    { label: '₹10,000 - ₹25,000', min: 10000, max: 25000 },
    { label: '₹25,000 - ₹50,000', min: 25000, max: 50000 },
    { label: 'Above ₹50,000', min: 50000, max: 100000 },
  ];

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const query = params['q'] || '';
      const category = params['category'] || 'all';
      this.selectedCategory.set(category);

      if (query) {
        this.searchQuery.set(query);
        this.performSearch(query);
      } else {
        this.loadAllProducts();
      }
    });
  }

  loadAllProducts(): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.hasSearched.set(false);

    this.productService.getAllProducts().subscribe({
      next: (products) => {
        this.allProducts.set(products);
        this.extractCategories(products);
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Load error:', err);
        this.error.set('Failed to load products. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  performSearch(query: string): void {
    if (!query.trim()) {
      this.loadAllProducts();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);
    this.hasSearched.set(true);

    this.productService.searchProducts(query).subscribe({
      next: (products) => {
        this.allProducts.set(products);
        this.extractCategories(products);
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Search error:', err);
        this.error.set('Search failed. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  onSearch(): void {
    const query = this.searchQuery();
    if (query.trim()) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { q: query },
        queryParamsHandling: 'merge',
      });
      this.performSearch(query);
    } else {
      this.loadAllProducts();
    }
  }

  onProductClick(product: ProductDetails): void {
    this.viewProductDetail(product);
  }

  viewProductDetail(product: ProductDetails): void {
    this.productStateService.setSelectedProduct(product);
    this.router.navigate(['/product-detail']);
  }

  buyNow(product: ProductDetails, event: Event): void {
    event.stopPropagation();

    if (!this.authState.isAuthenticated()) {
      this.productStateService.setSelectedProduct(product);
      this.redirectService.storeIntendedRoute('/payment', { fromProduct: 'true', quantity: 1 });
      toast.info('Please login to continue with your purchase');
      this.router.navigate(['/auth']);
      return;
    }

    this.productStateService.setSelectedProduct(product);
    this.router.navigate(['/payment'], { queryParams: { fromProduct: 'true', quantity: 1 } });
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
    this.loadAllProducts();
  }

  extractCategories(products: ProductDetails[]): void {
    const uniqueCategories = [...new Set(products.map((p) => p.categoryName))];
    this.categories.set(uniqueCategories);

    if (products.length > 0) {
      const prices = products.map((p) => p.price);
      const min = Math.floor(Math.min(...prices) / 100) * 100;
      const max = Math.ceil(Math.max(...prices) / 100) * 100;
      this.minPrice.set(min);
      this.maxPrice.set(max);
      this.priceRangeMin.set(min);
      this.priceRangeMax.set(max);
    }
  }

  selectCategory(category: string): void {
    this.selectedCategory.set(category);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { category: category === 'all' ? null : category },
      queryParamsHandling: 'merge',
    });
    this.applyFilters();
  }

  setPriceRange(min: number, max: number): void {
    this.priceRangeMin.set(min);
    this.priceRangeMax.set(max);
    this.applyFilters();
  }

  onPriceRangeChange(): void {
    this.applyFilters();
  }

  toggleFilters(): void {
    this.showFilters.update((v) => !v);
  }

  clearAllFilters(): void {
    this.selectedCategory.set('all');
    this.priceRangeMin.set(this.minPrice());
    this.priceRangeMax.set(this.maxPrice());
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
    this.applyFilters();
  }

  getActiveFiltersCount(): number {
    let count = 0;
    if (this.selectedCategory() !== 'all') count++;
    if (this.priceRangeMin() !== this.minPrice() || this.priceRangeMax() !== this.maxPrice()) count++;
    return count;
  }

  applyFilters(): void {
    let products = [...this.allProducts()];

    if (this.selectedCategory() !== 'all') {
      products = products.filter((p) => p.categoryName === this.selectedCategory());
    }

    products = products.filter(
      (p) => p.price >= this.priceRangeMin() && p.price <= this.priceRangeMax()
    );

    this.filteredProducts.set(products);
  }

  getAverageRating(product: ProductDetails): number | null {
    if (!product.review || product.review.length === 0) return null;
    const avg = product.review.reduce((sum, r) => sum + r.reviewPoints, 0) / product.review.length;
    return Math.round(avg * 10) / 10;
  }
}
