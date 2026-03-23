import { Component, inject, OnChanges, OnInit, signal, SimpleChanges } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { CartService } from '../../../services/cart.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { ApiResponse } from '../../../models/users/apiResponse.model';
import {
  CartItemDTO,
  GetCartResponseDTO,
  RemoveAllFromCartResponseDTO,
  RemoveFromCartRequestDTO,
  RemoveFromCartResponseDTO,
} from '../../../models/users/cart.model';
import { sign } from 'crypto';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-cart',
  imports: [MatIconModule],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart implements OnChanges, OnInit {
  private readonly cartId = signal<string>('');
  cartItems = signal<CartItemDTO[]>([]);
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
      next: (response: ApiResponse<GetCartResponseDTO>) => {
        this.cartId.set(response.data?.cartId ?? '');
        this.cartItems.set(response.data?.cartItems ?? []);
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

  removeAllCart() {
    this.apiService.removeAllFromCart().subscribe({
      next: (response: ApiResponse<RemoveAllFromCartResponseDTO>) => {
        console.log('response');
        console.log(response);
        toast.success(response.message);
      },
      error: (err) => {
        console.error(err);
        console.error(err?.error?.message ?? 'Remove all cart failed');
        toast.error(err?.error?.message ?? 'Remove all cart failed');
      },
      complete() {
        console.log('remove all cart completed');
      },
    });
    this.cartItems.set([]);
    this.currentPage = 1;
  }

  placeAllOrders() {
    console.log('Placing order for:', this.cartItems);
  }

  proceedToCheckout() {
    this.router.navigate(['/payment'], {
      queryParams: { fromCart: 'true' },
    });
  }

  increaseQty(item: any) {
    item.quantity++;
  }

  decreaseQty(item: any) {
    if (item.quantity > 1) {
      item.quantity--;
    }
  }

  removeFromCart(item: CartItemDTO) {
    console.log('item');
    console.log(item);
    const request: RemoveFromCartRequestDTO = {
      CartId: this.cartId(),
      CartItemId: item.cartItemId,
      ProductId: item.productId,
    };

    this.apiService.removeFromCart(request).subscribe({
      next: (response: ApiResponse<RemoveFromCartResponseDTO>) => {
        console.log('response');
        console.log(response);
        toast.success(response.message);
      },
      error: (err) => {
        console.error(err);
        console.error(err?.error?.message ?? 'Remove all cart failed');
        toast.error(err?.error?.message ?? 'Remove all cart failed');
      },
      complete() {
        console.log('remove from cart completed');
      },
    });
    console.log('request');
    console.log(request);
    this.cartItems.set(this.cartItems().filter((i) => i.cartItemId !== item.cartItemId));
  }

  get totalItems() {
    return this.cartItems().length;
  }

  get totalPrice() {
    return Number(
      this.cartItems()
        .reduce((sum, item) => sum + item.price * item.quantity, 0)
        .toFixed(2)
    );
  }

  get totalPages() {
    return Math.ceil(this.cartItems.length / this.itemsPerPage);
  }

  get pages() {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get paginatedItems() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.cartItems().slice(start, start + this.itemsPerPage);
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
