import { Injectable, signal } from '@angular/core';
import { ProductDetails } from '../../shared/models/users/product.model';

@Injectable({
  providedIn: 'root',
})
export class ProductStateService {
  private selectedProduct = signal<ProductDetails | null>(null);

  setSelectedProduct(product: ProductDetails): void {
    this.selectedProduct.set(product);
  }

  getSelectedProduct(): ProductDetails | null {
    return this.selectedProduct();
  }

  clearSelectedProduct(): void {
    this.selectedProduct.set(null);
  }
}
