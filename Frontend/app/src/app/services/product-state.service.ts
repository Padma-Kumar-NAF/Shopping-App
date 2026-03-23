import { Injectable, signal } from '@angular/core';
import { ProductItem } from '../models/users/product.model';

@Injectable({
  providedIn: 'root',
})
export class ProductStateService {
  private selectedProduct = signal<ProductItem | null>(null);

  setSelectedProduct(product: ProductItem): void {
    this.selectedProduct.set(product);
  }

  getSelectedProduct(): ProductItem | null {
    return this.selectedProduct();
  }

  clearSelectedProduct(): void {
    this.selectedProduct.set(null);
  }
}
