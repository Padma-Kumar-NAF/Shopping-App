import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import { ProductItem } from '../../../models/product.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-management.html',
  styleUrl: './product-management.css',
})
export class ProductManagement implements OnInit {
  private productService = inject(ProductService);

  products = signal<ProductItem[]>([]);
  filteredProducts = signal<ProductItem[]>([]);
  isLoading = signal<boolean>(false);
  searchQuery = signal<string>('');
  selectedCategory = signal<string>('all');
  categories = signal<string[]>([]);

  // Edit modal
  showEditModal = signal<boolean>(false);
  editingProduct = signal<ProductItem | null>(null);
  editForm = signal({
    name: '',
    category: '',
    price: 0,
    stock: 0,
    image: '',
    description: '',
  });

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.productService.getAllProducts().subscribe({
      next: (products) => {
        this.products.set(products);
        this.filteredProducts.set(products);
        this.extractCategories(products);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        toast.error('Failed to load products');
        this.isLoading.set(false);
      },
    });
  }

  extractCategories(products: ProductItem[]): void {
    const uniqueCategories = [...new Set(products.map((p) => p.category))];
    this.categories.set(uniqueCategories);
  }

  filterProducts(): void {
    let filtered = this.products();

    // Filter by search query
    if (this.searchQuery()) {
      const query = this.searchQuery().toLowerCase();
      filtered = filtered.filter(
        (p) =>
          p.name.toLowerCase().includes(query) ||
          p.category.toLowerCase().includes(query) ||
          p.description?.toLowerCase().includes(query)
      );
    }

    // Filter by category
    if (this.selectedCategory() !== 'all') {
      filtered = filtered.filter((p) => p.category === this.selectedCategory());
    }

    this.filteredProducts.set(filtered);
  }

  onSearchChange(): void {
    this.filterProducts();
  }

  selectCategory(category: string): void {
    this.selectedCategory.set(category);
    this.filterProducts();
  }

  openEditModal(product: ProductItem): void {
    this.editingProduct.set(product);
    this.editForm.set({
      name: product.name,
      category: product.category,
      price: product.price,
      stock: product.stock || 0,
      image: product.image,
      description: product.description || '',
    });
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.editingProduct.set(null);
  }

  saveProduct(): void {
    const product = this.editingProduct();
    if (!product) return;

    const form = this.editForm();

    // Validation
    if (!form.name || !form.category || form.price <= 0) {
      toast.error('Please fill all required fields');
      return;
    }

    // Update product in the list
    this.products.update((products) =>
      products.map((p) =>
        p.id === product.id
          ? {
              ...p,
              name: form.name,
              category: form.category,
              price: form.price,
              stock: form.stock,
              image: form.image,
              description: form.description,
            }
          : p
      )
    );

    this.filterProducts();
    toast.success('Product updated successfully!');
    this.closeEditModal();
  }

  deleteProduct(product: ProductItem): void {
    if (confirm(`Are you sure you want to delete "${product.name}"?`)) {
      this.products.update((products) => products.filter((p) => p.id !== product.id));
      this.filterProducts();
      toast.success('Product deleted successfully!');
    }
  }
}
