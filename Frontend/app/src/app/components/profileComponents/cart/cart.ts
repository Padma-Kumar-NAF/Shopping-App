import { Component, inject, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { CartService } from '../../../services/cart.service';
import { PaginationModel } from '../../../models/users/pagination.model';

@Component({
  selector: 'app-cart',
  imports: [MatIconModule],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart implements OnChanges, OnInit {
  constructor(private router: Router) {
    this.pagination = new PaginationModel();
    this.pagination.pageSize = 10;
    this.pagination.pageNumber = 1;
  }

  private readonly apiService: CartService = inject(CartService);
  ngOnChanges(changes: SimpleChanges): void {}

  ngOnInit(): void {
    this.getUserCarts();
  }

  getUserCarts() {
    this.apiService.GetUserCart(this.pagination).subscribe({
      next: (response) => {
        console.log('response');
        console.log(response);
      },
      error: (err) => {
        console.error(err);
      },
      complete() {
        console.log('getUserCarts completed');
      },
    });
  }

  pagination: PaginationModel;

  itemsPerPage = 6;
  currentPage = 1;

  cartItems = [
    {
      id: 1,
      name: 'Red Nail Polish',
      price: 799,
      quantity: 1,
      image: 'https://cdn.dummyjson.com/product-images/beauty/red-nail-polish/1.webp',
    },
    {
      id: 2,
      name: 'Nike Shoes',
      price: 4999,
      quantity: 2,
      image: 'https://cdn.dummyjson.com/product-images/fragrances/chanel-coco-noir-eau-de/1.webp',
    },
  ];
  removeAllCart() {
    this.cartItems = [];
    this.currentPage = 1;
  }

  placeAllOrders() {
    console.log('Placing order for:', this.cartItems);
  }

  proceedToCheckout() {
    this.router.navigate(['/checkout']);
  }

  increaseQty(item: any) {
    item.quantity++;
  }

  decreaseQty(item: any) {
    if (item.quantity > 1) {
      item.quantity--;
    }
  }

  removeFromCart(item: any) {
    this.cartItems = this.cartItems.filter((i) => i.id !== item.id);
  }

  get totalItems() {
    return this.cartItems.length;
  }

  get totalPrice() {
    return this.cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  get totalPages() {
    return Math.ceil(this.cartItems.length / this.itemsPerPage);
  }

  get pages() {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get paginatedItems() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.cartItems.slice(start, start + this.itemsPerPage);
  }

  goToPage(page: number) {
    this.currentPage = page;
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }
}
