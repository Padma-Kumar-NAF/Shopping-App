import { Component } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';

import { ProductCarousel } from '../product-carousel/product-carousel';
import { Footer } from '../footer/footer';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatCardModule,
    MatSelectModule,
    ProductCarousel,
    Footer,
  ],
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
})
export class HomeComponent {
  categories = [
    'Electronics',
    'Fashion',
    'Shoes',
    'Mobiles',
    'Laptops',
    'Beauty',
    'Home',
    'Sports',
  ];

  viewProduct(product: any) {
    console.log(product);
  }
  addToCart(product: any) {
    console.log(product);
  }
  buyProduct(product: any) {
    console.log(product);
  }

  products: any[] = Array.from({ length: 12 }).map((_, i) => ({
    name: `Product ${i + 1}`,
    price: Math.floor(Math.random() * 5000),
    image: `https://picsum.photos/300/200?random=${i}`,
  }));
}
