import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { AuthStateService } from '../../services/auth-state.service';
import { RedirectService } from '../../services/redirect.service';
import { ProductItem, SearchResult } from '../../models/users/product.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-product-listing',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-listing.html',
  styleUrl: './product-listing.css',
})
export class ProductListing implements OnInit {
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authState = inject(AuthStateService);
  private redirectService = inject(RedirectService);

  searchQuery = signal<string>('');
  searchResult = signal<SearchResult | null>(null);
  isLoading = signal<boolean>(false);
  hasSearched = signal<boolean>(false);
  selectedCategory = signal<string>('all');
  categories = signal<string[]>([]);
  filteredProducts = signal<ProductItem[]>([]);

  // Price filter signals
  minPrice = signal<number>(0);
  maxPrice = signal<number>(100000);
  priceRangeMin = signal<number>(0);
  priceRangeMax = signal<number>(100000);

  // Filter visibility
  showFilters = signal<boolean>(false);

  // Predefined price ranges
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
    // Check for query parameter
    this.route.queryParams.subscribe((params) => {
      const query = params['q'] || '';
      const category = params['category'] || 'all';

      this.selectedCategory.set(category);

      if (query) {
        this.searchQuery.set(query);
        this.performSearch(query);
      } else {
        // Load all products if no query
        this.loadAllProducts();
      }
    });
  }

  performSearch(query: string): void {
    if (!query.trim()) {
      this.loadAllProducts();
      return;
    }

    this.isLoading.set(true);
    this.hasSearched.set(true);

    this.productService.searchProducts(query).subscribe({
      next: (result) => {
        this.searchResult.set(result);
        this.extractCategories(result.relatedProducts);
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Search error:', error);
        this.isLoading.set(false);
      },
    });
  }

  loadAllProducts(): void {
    this.isLoading.set(true);
    this.productService.getAllProducts().subscribe({
      next: (products) => {
        this.searchResult.set({
          query: '',
          exactMatch: null,
          relatedProducts: products,
          totalResults: products.length,
        });
        this.extractCategories(products);
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Load error:', error);
        this.isLoading.set(false);
      },
    });
  }

  onSearch(): void {
    const query = this.searchQuery();
    if (query.trim()) {
      // Update URL with query parameter
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

  onProductClick(product: ProductItem): void {
    this.viewProductDetail(product);
  }

  viewProductDetail(product: ProductItem): void {
    this.router.navigate(['/product', product.id]);
  }

  buyNow(product: ProductItem, event: Event): void {
    event.stopPropagation();

    // Check if user is authenticated
    if (!this.authState.isAuthenticated()) {
      // Store the intended action
      this.redirectService.storeIntendedRoute('/payment', {
        productId: product.id,
        quantity: 1,
      });

      toast.info('Please login to continue with your purchase');
      this.router.navigate(['/auth']);
      return;
    }

    // User is authenticated, proceed to payment
    this.router.navigate(['/payment'], {
      queryParams: {
        productId: product.id,
        quantity: 1,
      },
    });
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {},
    });
    this.loadAllProducts();
  }

  getStarArray(rating: number): number[] {
    return Array(5)
      .fill(0)
      .map((_, i) => (i < Math.floor(rating) ? 1 : 0));
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  extractCategories(products: ProductItem[]): void {
    const uniqueCategories = [...new Set(products.map((p) => p.category))];
    this.categories.set(uniqueCategories);

    // Calculate price range from products
    if (products.length > 0) {
      const prices = products.map((p) => p.price);
      const min = Math.floor(Math.min(...prices) / 100) * 100; // Round down to nearest 100
      const max = Math.ceil(Math.max(...prices) / 100) * 100; // Round up to nearest 100
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
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {},
    });
    this.applyFilters();
  }

  getActiveFiltersCount(): number {
    let count = 0;
    if (this.selectedCategory() !== 'all') count++;
    if (this.priceRangeMin() !== this.minPrice() || this.priceRangeMax() !== this.maxPrice()) {
      count++;
    }
    return count;
  }

  applyFilters(): void {
    const result = this.searchResult();
    if (!result) return;

    let products = [...result.relatedProducts];

    // Apply category filter
    if (this.selectedCategory() !== 'all') {
      products = products.filter((p) => p.category === this.selectedCategory());
    }

    // Apply price range filter
    products = products.filter(
      (p) => p.price >= this.priceRangeMin() && p.price <= this.priceRangeMax()
    );

    this.filteredProducts.set(products);
  }
}
