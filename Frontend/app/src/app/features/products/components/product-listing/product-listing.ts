import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { ProductService } from '../../services/product.service';
import { ProductStateService } from '../../../../core/state/product-state.service';
import { AuthStateService } from '../../../../core/state/auth-state.service';
import { RedirectService } from '../../../../core/services/redirect.service';
import { AdminCategoryService } from '../../../../features/admin/services/category.service';
import { ProductDetails } from '../../../../shared/models/users/product.model';
import { CategoryDTO } from '../../../../shared/models/admin/categories.model';
import { PaginationModel } from '../../../../shared/models/users/pagination.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-product-listing',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-listing.html',
  styleUrl: './product-listing.css',
})
export class ProductListing implements OnInit, OnDestroy {
  private productService = inject(ProductService);
  private productStateService = inject(ProductStateService);
  private categoryService = inject(AdminCategoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authState = inject(AuthStateService);
  private redirectService = inject(RedirectService);

  private destroy$ = new Subject<void>();
  private filterChange$ = new Subject<void>();

  searchQuery = signal<string>('');
  filteredProducts = signal<ProductDetails[]>([]);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);
  hasSearched = signal<boolean>(false);

  categories = signal<CategoryDTO[]>([]);
  selectedCategoryId = signal<string | null>(null);
  selectedCategoryName = signal<string>('all');

  readonly PRICE_MIN = 0;
  readonly PRICE_MAX = 100000;
  priceRangeMin = signal<number>(this.PRICE_MIN);
  priceRangeMax = signal<number>(this.PRICE_MAX);

  showFilters = signal<boolean>(false);

  priceRanges = [
    { label: 'All Prices', min: 0, max: 100000 },
    { label: 'Under ₹1,000', min: 0, max: 1000 },
    { label: '₹1,000 – ₹5,000', min: 1000, max: 5000 },
    { label: '₹5,000 – ₹10,000', min: 5000, max: 10000 },
    { label: '₹10,000 – ₹25,000', min: 10000, max: 25000 },
    { label: '₹25,000 – ₹50,000', min: 25000, max: 50000 },
    { label: 'Above ₹50,000', min: 50000, max: 100000 },
  ];

  ngOnInit(): void {
    this.filterChange$
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => this.fetchFiltered());
    this.loadCategories();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCategories(): void {
    const pagination = new PaginationModel();
    pagination.pageSize = 100;
    pagination.pageNumber = 1;

    this.categoryService.getAllCategories(pagination).subscribe({
      next: (res) => {
        this.categories.set(res.data?.categoryList ?? []);
        this.subscribeToQueryParams();
      },
      error: (err) => {
        console.error(err);
        this.subscribeToQueryParams();
      },
    });
  }

  private subscribeToQueryParams(): void {
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const query = params['q'] || '';
      const catName = params['category'] || 'all';

      this.searchQuery.set(query);
      this.applyCategoryByName(catName);

      if (query) {
        this.hasSearched.set(true);
        this.performSearch(query);
      } else {
        this.hasSearched.set(false);
        this.fetchFiltered();
      }
    });
  }

  private applyCategoryByName(name: string): void {
    if (name === 'all') {
      this.selectedCategoryId.set(null);
      this.selectedCategoryName.set('all');
      return;
    }
    const match = this.categories().find(
      (c) => c.categoryName.toLowerCase() === name.toLowerCase()
    );
    this.selectedCategoryId.set(match?.categoryId ?? null);
    this.selectedCategoryName.set(name);
  }

  fetchFiltered(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const request = {
      pagination: { pageSize: 50, pageNumber: 1 },
      lowPrice: this.priceRangeMin(),
      highPrice: this.priceRangeMax(),
      categoryId: this.selectedCategoryId() ?? null,
    };

    this.productService.getProductsWithFilter(request).subscribe({
      next: (res) => {
        if (res.action === 'ShowEmptyPage') {
          this.filteredProducts.set([]);
        } else {
          this.filteredProducts.set(res.data?.productList ?? []);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.message || 'Failed to load products. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  performSearch(query: string): void {
    if (!query.trim()) {
      this.fetchFiltered();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    this.productService.searchProducts(query).subscribe({
      next: (products) => {
        let result = products.filter(
          (p) => p.price >= this.priceRangeMin() && p.price <= this.priceRangeMax()
        );
        if (this.selectedCategoryId()) {
          result = result.filter((p) => p.categoryId === this.selectedCategoryId());
        }
        this.filteredProducts.set(result);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.message || 'Search failed. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  selectCategory(categoryId: string | null, categoryName: string): void {
    this.selectedCategoryId.set(categoryId);
    this.selectedCategoryName.set(categoryName);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { category: categoryName === 'all' ? null : categoryName },
      queryParamsHandling: 'merge',
    });
    this.fetchFiltered();
  }

  setPriceRange(min: number, max: number): void {
    this.priceRangeMin.set(min);
    this.priceRangeMax.set(max);
    this.fetchFiltered();
  }

  onPriceSliderChange(): void {
    this.filterChange$.next();
  }

  clearAllFilters(): void {
    this.selectedCategoryId.set(null);
    this.selectedCategoryName.set('all');
    this.priceRangeMin.set(this.PRICE_MIN);
    this.priceRangeMax.set(this.PRICE_MAX);
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
    this.fetchFiltered();
  }

  onSearch(): void {
    const query = this.searchQuery();
    if (query.trim()) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { q: query },
        queryParamsHandling: 'merge',
      });
      this.hasSearched.set(true);
      this.performSearch(query);
    } else {
      this.clearSearch();
    }
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.hasSearched.set(false);
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
    this.fetchFiltered();
  }

  toggleFilters(): void {
    this.showFilters.update((v) => !v);
  }

  getActiveFiltersCount(): number {
    let count = 0;
    if (this.selectedCategoryId() !== null) count++;
    if (this.priceRangeMin() !== this.PRICE_MIN || this.priceRangeMax() !== this.PRICE_MAX) count++;
    return count;
  }

  onProductClick(product: ProductDetails): void {
    this.viewProductDetail(product);
  }

  viewProductDetail(product: ProductDetails): void {
    this.productStateService.setSelectedProduct(product);
    this.router.navigate(['/product-detail', product.productId]);
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

  getAverageRating(product: ProductDetails): number | null {
    if (!product.review || product.review.length === 0) return null;
    const avg = product.review.reduce((sum, r) => sum + r.reviewPoints, 0) / product.review.length;
    return Math.round(avg * 10) / 10;
  }
}
