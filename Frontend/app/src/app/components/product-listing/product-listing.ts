import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { ProductItem, SearchResult } from '../../models/product.model';

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

  searchQuery = signal<string>('');
  searchResult = signal<SearchResult | null>(null);
  isLoading = signal<boolean>(false);
  hasSearched = signal<boolean>(false);

  ngOnInit(): void {
    // Check for query parameter
    this.route.queryParams.subscribe((params) => {
      const query = params['q'] || '';
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
    console.log('Product clicked:', product);
    this.viewProductDetail(product);
  }

  viewProductDetail(product: ProductItem): void {
    console.log('Navigating to product detail:', product);
    // Navigate to product detail page with product ID
    this.router.navigate(['/product', product.id]);
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
}
