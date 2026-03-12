import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ProductCarousel } from '../product-carousel/product-carousel';
import { Footer } from '../footer/footer';
import { FormsModule } from '@angular/forms';

interface Product {
  id: number;
  name: string;
  price: number;
  image: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ProductCarousel, Footer, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
})
export class HomeComponent {
  categories: string[] = [
    'Electronics',
    'Fashion',
    'Shoes',
    'Mobiles',
    'Laptops',
    'Beauty',
    'Home & Kitchen',
    'Sports',
    'Books',
    'Toys',
  ];

  products: Product[] = Array.from({ length: 12 }).map((_, i) => ({
    id: i + 1,
    name: `Product ${i + 1}`,
    price: Math.floor(Math.random() * 5000) + 500,
    image: `https://picsum.photos/300/200?random=${i}`,
  }));

  selectedProduct: Product | null = null;
  searchQuery = signal<string>('');

  constructor(private router: Router) {}

  onProductClick(product: Product): void {
    console.log('Product clicked:', product);
    // Navigate to product detail page
    this.router.navigate(['/product', product.id]);
  }

  clearSelection(): void {
    this.selectedProduct = null;
  }

  navigateToCart(): void {
    console.log('Navigating to Cart');
    // this.router.navigate(['/cart']);
  }

  navigateToProfile(): void {
    console.log('Navigating to Profile');
    // this.router.navigate(['/profile']);
  }

  onCategoryClick(category: string): void {
    console.log('Category clicked:', category);
    // Navigate to product listing with category filter
    this.router.navigate(['/products'], { queryParams: { q: category } });
  }

  onSearch(): void {
    const query = this.searchQuery();
    if (query.trim()) {
      this.router.navigate(['/products'], { queryParams: { q: query } });
    }
  }
}
