import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Footer } from '../footer/footer';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import { ProductStateService } from '../../../services/product-state.service';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { ProductDetails } from '../../../models/users/product.model';
import { CategoryDTO } from '../../../models/admin/categories.model';
import { PaginationModel } from '../../../models/users/pagination.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [Footer, CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
})
export class HomeComponent implements OnInit {
  private router = inject(Router);
  private productService = inject(ProductService);
  private categoryService = inject(AdminCategoryService);
  private productStateService = inject(ProductStateService);

  searchQuery = signal<string>('');
  products = signal<ProductDetails[]>([]);
  categories = signal<CategoryDTO[]>([]);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  carouselSlides = [
    {
      image: 'https://images.unsplash.com/photo-1607082349566-187342175e2f?w=1280&h=420&fit=crop',
      title: 'Mega Sale — Up to 60% Off',
      subtitle: "Grab the best deals before they're gone!",
    },
    {
      image: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1280&h=420&fit=crop',
      title: 'New Season Arrivals',
      subtitle: 'Fresh looks for every style',
    },
    {
      image: 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=1280&h=420&fit=crop',
      title: 'Top Electronics Picks',
      subtitle: 'Power up your life with the latest gadgets',
    },
    {
      image: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=1280&h=420&fit=crop',
      title: 'Sneaker Drop 2025',
      subtitle: 'Step into comfort and style',
    },
    {
      image: 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=1280&h=420&fit=crop',
      title: 'Home & Living',
      subtitle: 'Transform your spaces effortlessly',
    },
    {
      image: 'https://images.unsplash.com/photo-1490481651871-ab68de25d43d?w=1280&h=420&fit=crop',
      title: 'Fashion Week Specials',
      subtitle: 'Curated trends at unbeatable prices',
    },
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const pagination = new PaginationModel();
    pagination.pageSize = 20;
    pagination.pageNumber = 1;

    forkJoin({
      products: this.productService.getAllProducts(20, 1),
      categories: this.categoryService.getAllCategories(pagination),
    }).subscribe({
      next: ({ products, categories }) => {
        this.products.set(products);
        this.categories.set(categories.data?.categoryList ?? []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.message || 'Failed to load data. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  onProductClick(product: ProductDetails): void {
    this.productStateService.setSelectedProduct(product);
    this.router.navigate(['/product-detail', product.productId]);
  }

  onCategoryClick(category: CategoryDTO): void {
    this.router.navigate(['/products'], { queryParams: { category: category.categoryName } });
  }

  onCategorySelect(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    if (value) {
      this.router.navigate(['/products'], { queryParams: { category: value } });
    } else {
      this.router.navigate(['/products']);
    }
  }

  onSearch(): void {
    const query = this.searchQuery();
    if (query.trim()) {
      this.router.navigate(['/products'], { queryParams: { q: query } });
    }
  }

  getAverageRating(product: ProductDetails): number | null {
    if (!product.review?.length) return null;
    const avg = product.review.reduce((s, r) => s + r.reviewPoints, 0) / product.review.length;
    return Math.round(avg * 10) / 10;
  }
}
