import { Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { ProductItem, SearchResult } from '../models/users/product.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private products: ProductItem[] = [
    {
      id: '1',
      name: 'Wireless Bluetooth Headphones',
      category: 'Electronics',
      price: 2999,
      image: 'https://picsum.photos/400/300?random=1',
      description: 'Premium wireless headphones with noise cancellation',
      rating: 4.5,
      stock: 50,
      tags: ['wireless', 'bluetooth', 'headphones', 'audio', 'music'],
    },
    {
      id: '2',
      name: 'Smart Watch Pro',
      category: 'Electronics',
      price: 8999,
      image: 'https://picsum.photos/400/300?random=2',
      description: 'Advanced smartwatch with health tracking',
      rating: 4.7,
      stock: 30,
      tags: ['smartwatch', 'fitness', 'health', 'wearable'],
    },
    {
      id: '3',
      name: 'Laptop Gaming Pro',
      category: 'Laptops',
      price: 75999,
      image: 'https://picsum.photos/400/300?random=3',
      description: 'High-performance gaming laptop',
      rating: 4.8,
      stock: 15,
      tags: ['laptop', 'gaming', 'computer', 'pc'],
    },
    {
      id: '4',
      name: 'Running Shoes Ultra',
      category: 'Shoes',
      price: 3499,
      image: 'https://picsum.photos/400/300?random=4',
      description: 'Comfortable running shoes for athletes',
      rating: 4.3,
      stock: 100,
      tags: ['shoes', 'running', 'sports', 'footwear'],
    },
    {
      id: '5',
      name: 'Smartphone X Pro',
      category: 'Mobiles',
      price: 45999,
      image: 'https://picsum.photos/400/300?random=5',
      description: 'Latest smartphone with advanced camera',
      rating: 4.6,
      stock: 40,
      tags: ['smartphone', 'mobile', 'phone', 'android'],
    },
    {
      id: '6',
      name: 'Wireless Mouse',
      category: 'Electronics',
      price: 899,
      image: 'https://picsum.photos/400/300?random=6',
      description: 'Ergonomic wireless mouse',
      rating: 4.2,
      stock: 80,
      tags: ['mouse', 'wireless', 'computer', 'accessory'],
    },
    {
      id: '7',
      name: 'Mechanical Keyboard RGB',
      category: 'Electronics',
      price: 4999,
      image: 'https://picsum.photos/400/300?random=7',
      description: 'RGB mechanical keyboard for gaming',
      rating: 4.7,
      stock: 25,
      tags: ['keyboard', 'mechanical', 'gaming', 'rgb'],
    },
    {
      id: '8',
      name: 'Coffee Maker Deluxe',
      category: 'Home & Kitchen',
      price: 5999,
      image: 'https://picsum.photos/400/300?random=8',
      description: 'Automatic coffee maker with timer',
      rating: 4.4,
      stock: 35,
      tags: ['coffee', 'maker', 'kitchen', 'appliance'],
    },
    {
      id: '9',
      name: 'Yoga Mat Premium',
      category: 'Sports',
      price: 1299,
      image: 'https://picsum.photos/400/300?random=9',
      description: 'Non-slip yoga mat for fitness',
      rating: 4.5,
      stock: 60,
      tags: ['yoga', 'mat', 'fitness', 'exercise'],
    },
    {
      id: '10',
      name: 'Backpack Travel Pro',
      category: 'Fashion',
      price: 2499,
      image: 'https://picsum.photos/400/300?random=10',
      description: 'Durable travel backpack with multiple compartments',
      rating: 4.6,
      stock: 45,
      tags: ['backpack', 'travel', 'bag', 'fashion'],
    },
    {
      id: '11',
      name: 'Bluetooth Speaker Portable',
      category: 'Electronics',
      price: 1999,
      image: 'https://picsum.photos/400/300?random=11',
      description: 'Waterproof portable bluetooth speaker',
      rating: 4.4,
      stock: 70,
      tags: ['speaker', 'bluetooth', 'portable', 'audio'],
    },
    {
      id: '12',
      name: 'Digital Camera 4K',
      category: 'Electronics',
      price: 35999,
      image: 'https://picsum.photos/400/300?random=12',
      description: 'Professional 4K digital camera',
      rating: 4.8,
      stock: 20,
      tags: ['camera', 'digital', '4k', 'photography'],
    },
  ];

  constructor() {}

  searchProducts(query: string): Observable<SearchResult> {
    const normalizedQuery = query.toLowerCase().trim();

    if (!normalizedQuery) {
      return of({
        query: query,
        exactMatch: null,
        relatedProducts: this.products,
        totalResults: this.products.length,
      }).pipe(delay(300));
    }

    const exactMatch = this.products.find((p) => p.name.toLowerCase() === normalizedQuery);

    const relatedProducts = this.products.filter((p) => {
      if (exactMatch && p.id === exactMatch.id) return false;

      const matchesName = p.name.toLowerCase().includes(normalizedQuery);
      const matchesCategory = p.category.toLowerCase().includes(normalizedQuery);
      const matchesTags = p.tags?.some((tag) => tag.toLowerCase().includes(normalizedQuery));
      const matchesDescription = p.description?.toLowerCase().includes(normalizedQuery);

      return matchesName || matchesCategory || matchesTags || matchesDescription;
    });

    relatedProducts.sort((a, b) => {
      const aScore = this.calculateRelevanceScore(a, normalizedQuery);
      const bScore = this.calculateRelevanceScore(b, normalizedQuery);
      return bScore - aScore;
    });

    const result: SearchResult = {
      query: query,
      exactMatch: exactMatch || null,
      relatedProducts: relatedProducts,
      totalResults: (exactMatch ? 1 : 0) + relatedProducts.length,
    };

    return of(result).pipe(delay(300));
  }

  private calculateRelevanceScore(product: ProductItem, query: string): number {
    let score = 0;

    if (product.name.toLowerCase().includes(query)) {
      score += 10;
      if (product.name.toLowerCase().startsWith(query)) {
        score += 5;
      }
    }

    if (product.category.toLowerCase().includes(query)) {
      score += 5;
    }

    const tagMatches = product.tags?.filter((tag) => tag.toLowerCase().includes(query)).length || 0;
    score += tagMatches * 3;

    if (product.description?.toLowerCase().includes(query)) {
      score += 2;
    }

    score += (product.rating || 0) * 0.5;

    return score;
  }

  getAllProducts(): Observable<ProductItem[]> {
    return of(this.products).pipe(delay(300));
  }

  getProductById(id: string): Observable<ProductItem | undefined> {
    const product = this.products.find((p) => p.id === id);
    return of(product).pipe(delay(300));
  }

  getProductsByCategory(category: string): Observable<ProductItem[]> {
    const filtered = this.products.filter(
      (p) => p.category.toLowerCase() === category.toLowerCase()
    );
    return of(filtered).pipe(delay(300));
  }
}
