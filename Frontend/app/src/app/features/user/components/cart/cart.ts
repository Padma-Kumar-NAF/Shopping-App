import { Component, inject, OnInit, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { CartService } from '../../../services/userServices/cart.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { ApiResponse } from '../../../models/users/apiResponse.model';
import {
  CartItemDTO,
  GetCartResponseDTO,
  RemoveAllFromCartResponseDTO,
  RemoveFromCartRequestDTO,
  RemoveFromCartResponseDTO,
  UpdateUserCartRequestDTO,
} from '../../../models/users/cart.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-cart',
  imports: [MatIconModule],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart implements OnInit {
  private readonly cartId = signal<string>('');
  cartItems = signal<CartItemDTO[]>([]);
  isLoading = signal<boolean>(false);

  private readonly apiService = inject(CartService);
  private readonly router = inject(Router);

  pagination: PaginationModel;
  itemsPerPage = 6;
  currentPage = 1;

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageSize = 18;
    this.pagination.pageNumber = 1;
  }

  ngOnInit(): void {
    this.getUserCarts();
  }

  getUserCarts(): void {
    this.isLoading.set(true);
    this.apiService.GetUserCart(this.pagination).subscribe({
      next: (response: ApiResponse<GetCartResponseDTO>) => {
        this.cartId.set(response.data?.cartId ?? '');
        this.cartItems.set(response.data?.cartItems ?? []);
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to load cart');
        this.isLoading.set(false);
      },
    });
  }

  removeAllCart(): void {
    this.apiService.removeAllFromCart().subscribe({
      next: (response: ApiResponse<RemoveAllFromCartResponseDTO>) => {
        toast.success(response.message || 'Cart cleared');
        this.cartItems.set([]);
        this.currentPage = 1;
      },
      error: (err) => toast.error(err?.error?.message ?? 'Remove all cart failed'),
    });
  }

  proceedToCheckout(): void {
    this.router.navigate(['/payment'], { queryParams: { fromCart: 'true' } });
  }

  increaseQty(item: CartItemDTO): void {
    const newQty = item.quantity + 1;
    this.updateCartItem(item, newQty);
  }

  decreaseQty(item: CartItemDTO): void {
    if (item.quantity <= 1) return;
    const newQty = item.quantity - 1;
    this.updateCartItem(item, newQty);
  }

  private updateCartItem(item: CartItemDTO, newQty: number): void {
    const request = new UpdateUserCartRequestDTO();
    request.cartId = this.cartId();
    request.cartItemId = item.cartItemId;
    request.productId = item.productId;
    request.quantity = newQty;

    this.apiService.updateCart(request).subscribe({
      next: (res) => {
        if (res.data?.isUpdated) {
          this.cartItems.update((items) =>
            items.map((i) => (i.cartItemId === item.cartItemId ? { ...i, quantity: newQty } : i))
          );
        }
      },
      error: (err) => toast.error(err?.error?.message || 'Failed to update quantity'),
    });
  }

  removeFromCart(item: CartItemDTO): void {
    const request: RemoveFromCartRequestDTO = {
      CartId: this.cartId(),
      CartItemId: item.cartItemId,
      ProductId: item.productId,
    };
    this.apiService.removeFromCart(request).subscribe({
      next: (response: ApiResponse<RemoveFromCartResponseDTO>) => {
        toast.success(response.message || 'Item removed');
        this.cartItems.update((items) => items.filter((i) => i.cartItemId !== item.cartItemId));
      },
      error: (err) => toast.error(err?.error?.message ?? 'Remove from cart failed'),
    });
  }

  get totalItems(): number {
    return this.cartItems().length;
  }

  get totalPrice(): number {
    return Number(
      this.cartItems().reduce((sum, item) => sum + item.price * item.quantity, 0).toFixed(2)
    );
  }

  get totalPages(): number {
    return Math.ceil(this.cartItems().length / this.itemsPerPage);
  }

  get paginatedItems(): CartItemDTO[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.cartItems().slice(start, start + this.itemsPerPage);
  }

  goToPage(page: number): void { this.currentPage = page; }
  nextPage(): void { if (this.currentPage < this.totalPages) this.currentPage++; }
  prevPage(): void { if (this.currentPage > 1) this.currentPage--; }
}
