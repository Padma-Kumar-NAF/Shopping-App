import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { toast } from 'ngx-sonner';

interface ProductDetails {
  id: number;
  name: string;
  price: number;
  image: string;
}

interface Wishlist {
  id: number;
  name: string;
  products: ProductDetails[];
}

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './wishlist.html',
  styleUrls: ['./wishlist.css'],
})

export class WishlistComponent {
  wishLists = signal<Wishlist[]>([
    {
      id: 1,
      name: 'Dream Wardrobe',
      products: [
        {
          id: 101,
          name: 'Premium Leather Jacket',
          price: 4999,
          image: 'https://images.unsplash.com/photo-1551028719-00167b16eac5?w=400&h=500&fit=crop',
        },
        {
          id: 102,
          name: 'Designer High-Top Sneakers',
          price: 2999,
          image: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=400&fit=crop',
        },
        {
          id: 103,
          name: 'Slim Fit Chinos',
          price: 1899,
          image: 'https://images.unsplash.com/photo-1551024506-0bccd828d307?w=400&h=500&fit=crop',
        },
      ],
    },
    {
      id: 2,
      name: 'Tech Gadgets',
      products: [
        {
          id: 201,
          name: 'Wireless Noise-Cancelling Headphones',
          price: 12999,
          image:
            'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&h=400&fit=crop',
        },
      ],
    },
  ]);

  selectedWishlistId = signal<number>(1);

  selectedWishlist = computed(
    () => this.wishLists().find((w) => w.id === this.selectedWishlistId()) || null,
  );

  isEmpty = computed(() => this.wishLists().length === 0);

  selectWishlist(id: number) {
    this.selectedWishlistId.set(id);
  }

  removeWishlist(id: number) {
    const wishlistName = this.wishLists().find(w => w.id == id)?.name
    toast.success(`${wishlistName} removed`)
    this.wishLists.update((lists) => lists.filter((w) => w.id !== id));

    if (id === this.selectedWishlistId()) {
      const remainingLists = this.wishLists();
      this.selectedWishlistId.set(remainingLists.length > 0 ? remainingLists[0].id : 0);
    }
  }

  removeProduct(wishlistId: number, productId: number) {
    const productName = this.wishLists().find(w => w.id == wishlistId)?.products.find(p => p.id == productId)?.name
    toast.success(`${productName} removed`)
    this.wishLists.update((lists) =>
      lists.map((w) =>
        w.id === wishlistId ? { ...w, products: w.products.filter((p) => p.id !== productId) } : w,
      ),
    );
  }
}
