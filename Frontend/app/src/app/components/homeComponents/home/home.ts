import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
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
  imports: [ Footer, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
})

export class HomeComponent {
  carouselSlides = [
    {
      image: 'https://images.unsplash.com/photo-1607082349566-187342175e2f?w=1280&h=420&fit=crop',
      title: 'Mega Sale — Up to 60% Off',
      subtitle: 'Grab the best deals before they\'re gone!',
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

  constructor(private router: Router) {
    
  }

  onProductClick(product: Product): void {  
    console.log('Product clicked:', product);
    this.router.navigate(['/product', product.id]);
  }

  clearSelection(): void {
    this.selectedProduct = null;
  }

  navigateToCart(): void {
    console.log('Navigating to Cart');
    this.router.navigate(['/cart']);
  }

  navigateToProfile(): void {
    console.log('Navigating to Profile');
    this.router.navigate(['/profile']);
  }

  onCategoryClick(category: string): void {
    console.log('Category clicked:', category);
    this.router.navigate(['/products'], { queryParams: { q: category } });
  }

  onSearch(): void {
    const query = this.searchQuery();
    if (query.trim()) {
      this.router.navigate(['/products'], { queryParams: { q: query } });
    }
  }
}
