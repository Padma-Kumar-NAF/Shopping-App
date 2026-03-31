import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { WishlistService, WishListDTO } from '../../../services/userServices/wishlist.service';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './wishlist.html',
  styleUrls: ['./wishlist.css'],
})
export class WishlistComponent implements OnInit {
  private wishlistService = inject(WishlistService);

  wishLists = signal<WishListDTO[]>([]);
  selectedWishlistId = signal<string>('');
  isLoading = signal<boolean>(false);
  showCreateForm = signal<boolean>(false);
  newWishlistName = signal<string>('');

  selectedWishlist = computed(
    () => this.wishLists().find((w) => w.wishListId === this.selectedWishlistId()) ?? null
  );

  isEmpty = computed(() => this.wishLists().length === 0);

  ngOnInit(): void {
    this.loadWishlists();
  }

  loadWishlists(): void {
    this.isLoading.set(true);
    this.wishlistService.getUserWishlists().subscribe({
      next: (res) => {
        const lists = res.data?.wishList ?? [];
        this.wishLists.set(lists);
        if (lists.length > 0 && !this.selectedWishlistId()) {
          this.selectedWishlistId.set(lists[0].wishListId);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to load wishlists');
        this.isLoading.set(false);
      },
    });
  }

  selectWishlist(id: string): void {
    this.selectedWishlistId.set(id);
  }

  createWishlist(): void {
    const name = this.newWishlistName().trim();
    if (!name) { toast.error('Please enter a wishlist name'); return; }
    this.wishlistService.createWishlist(name).subscribe({
      next: (res) => {
        if (res.data?.isCreated) {
          toast.success(`"${name}" created`);
          this.newWishlistName.set('');
          this.showCreateForm.set(false);
          this.loadWishlists();
        } else {
          toast.error(res.message || 'Failed to create wishlist');
        }
      },
      error: (err) => toast.error(err?.error?.message || 'Failed to create wishlist'),
    });
  }

  removeWishlist(wishListId: string): void {
    const name = this.wishLists().find((w) => w.wishListId === wishListId)?.wishListName;
    this.wishlistService.deleteWishlist(wishListId).subscribe({
      next: (res) => {
        if (res.data?.isDeleted) {
          toast.success(`"${name}" removed`);
          this.wishLists.update((lists) => lists.filter((w) => w.wishListId !== wishListId));
          if (wishListId === this.selectedWishlistId()) {
            const remaining = this.wishLists();
            this.selectedWishlistId.set(remaining.length > 0 ? remaining[0].wishListId : '');
          }
        } else {
          toast.error(res.message || 'Failed to delete wishlist');
        }
      },
      error: (err) => toast.error(err?.error?.message || 'Failed to delete wishlist'),
    });
  }

  removeProduct(wishListId: string, productId: string): void {
    const productName = this.wishLists()
      .find((w) => w.wishListId === wishListId)
      ?.wishListItems.find((p) => p.productId === productId)?.productName;

    this.wishlistService.removeProduct(wishListId, productId).subscribe({
      next: (res) => {
        if (res.data?.isRemoved) {
          toast.success(`"${productName}" removed`);
          this.wishLists.update((lists) =>
            lists.map((w) =>
              w.wishListId === wishListId
                ? { ...w, wishListItems: w.wishListItems.filter((p) => p.productId !== productId) }
                : w
            )
          );
        } else {
          toast.error(res.message || 'Failed to remove product');
        }
      },
      error: (err) => toast.error(err?.error?.message || 'Failed to remove product'),
    });
  }
}
