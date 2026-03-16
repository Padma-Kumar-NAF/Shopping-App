import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Product } from '../../../models/admin.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-product.html',
  styleUrl: './add-product.css',
})
export class AddProduct {
  categories = signal<string[]>([
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
  ]);

  product = signal<Product>({
    name: '',
    category: '',
    quantity: 0,
    imageUrl: '',
    price: 0,
    description: '',
  });

  imagePreview = signal<string>('');

  updateProductField(field: keyof Product, value: any): void {
    this.product.update((p) => ({ ...p, [field]: value }));
  }

  updateImageUrl(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateProductField('imageUrl', value);
    this.imagePreview.set(value);
  }

  validateForm(): boolean {
    const p = this.product();
    if (!p.name.trim()) {
      toast.error('Product name is required');
      return false;
    }
    if (!p.category) {
      toast.error('Please select a category');
      return false;
    }
    if (p.quantity <= 0) {
      toast.error('Quantity must be greater than 0');
      return false;
    }
    if (p.price <= 0) {
      toast.error('Price must be greater than 0');
      return false;
    }
    if (!p.imageUrl.trim()) {
      toast.error('Image URL is required');
      return false;
    }
    return true;
  }

  addProduct(): void {
    if (!this.validateForm()) {
      return;
    }

    const newProduct: Product = {
      ...this.product(),
      id: Date.now().toString(),
      createdAt: new Date(),
    };

    console.log('Adding product:', newProduct);
    toast.success('Product added successfully!');

    // Reset form
    this.product.set({
      name: '',
      category: '',
      quantity: 0,
      imageUrl: '',
      price: 0,
      description: '',
    });
    this.imagePreview.set('');
  }

  resetForm(): void {
    this.product.set({
      name: '',
      category: '',
      quantity: 0,
      imageUrl: '',
      price: 0,
      description: '',
    });
    this.imagePreview.set('');
    toast.info('Form reset');
  }
}
