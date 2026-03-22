import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toast } from 'ngx-sonner';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { StoreService } from '../../../services/adminServices/store.service';
import { AdminProductService } from '../../../services/adminServices/products.service';
import { PaginationModel } from '../../../models/admin/pagination.model';
import { ApiResponse } from '../../../models/admin/apiResponse.model';
import {
  ProductDetails,
  AddNewProductRequestDTO,
  AddNewProductResponseDTO,
  UpdateProductRequestDTO,
  UpdateProductResponseDTO,
  ReviewDTO,
} from '../../../models/admin/products.model';
import { CategoryDTO } from '../../../models/admin/categories.model';

type ActiveView = 'list' | 'add';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-management.html',
  styleUrl: './product-management.css',
})
export class ProductManagement implements OnInit {
  store = inject(StoreService);
  productService = inject(AdminProductService);

  // ── View toggle ───────────────────────────────────────────────────────────
  activeView = signal<ActiveView>('list');

  // ── Store observables ─────────────────────────────────────────────────────
  products$!: Observable<ProductDetails[]>;
  categories$!: Observable<CategoryDTO[]>;
  filteredProducts$!: Observable<ProductDetails[]>;

  // ── Filters ───────────────────────────────────────────────────────────────
  searchQuery = signal<string>('');
  selectedCategoryId = signal<string>('all');

  // ── Add form ──────────────────────────────────────────────────────────────
  addForm = signal<AddNewProductRequestDTO>({
    categoryId: '',
    name: '',
    imagePath: '',
    description: '',
    price: 0,
    quantity: 0,
  });
  imagePreview = signal<string>('');

  // ── Edit modal ────────────────────────────────────────────────────────────
  showEditModal = signal<boolean>(false);
  editingProduct = signal<ProductDetails | null>(null);
  editForm = signal<UpdateProductRequestDTO>({
    productId: '',
    categoryId: '',
    name: '',
    imagePath: '',
    description: '',
    price: 0,
    quantity: 0,
  });
  editImagePreview = signal<string>('');

  // ── Delete modal ──────────────────────────────────────────────────────────
  confirmModal = signal<boolean>(false);
  pendingDeleteId = signal<string>('');

  pagination: PaginationModel;

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = 50;
  }

  ngOnInit(): void {
    this.products$ = this.store.state$.pipe(map(s => s.products));
    this.categories$ = this.store.state$.pipe(map(s => s.categories));

    this.filteredProducts$ = this.store.state$.pipe(
      map(s => {
        let filtered = s.products;

        const q = this.searchQuery().toLowerCase();
        if (q) {
          filtered = filtered.filter(
            p =>
              p.productName.toLowerCase().includes(q) ||
              p.categoryName.toLowerCase().includes(q) ||
              p.description.toLowerCase().includes(q),
          );
        }

        if (this.selectedCategoryId() !== 'all') {
          filtered = filtered.filter(p => p.categoryId === this.selectedCategoryId());
        }

        return filtered;
      }),
    );
  }

  private refreshFilter(): void {
    this.store.setProducts([...this.store.value.products]);
  }

  // ── View ──────────────────────────────────────────────────────────────────
  switchView(view: ActiveView): void {
    this.activeView.set(view);
    if (view === 'add') this.resetAddForm();
  }

  // ── Filters ───────────────────────────────────────────────────────────────
  onSearchChange(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
    this.refreshFilter();
  }

  onCategoryChange(event: Event): void {
    this.selectedCategoryId.set((event.target as HTMLSelectElement).value);
    this.refreshFilter();
  }

  // ── Add ───────────────────────────────────────────────────────────────────
  updateAddField<K extends keyof AddNewProductRequestDTO>(
    field: K,
    value: AddNewProductRequestDTO[K],
  ): void {
    this.addForm.update(f => ({ ...f, [field]: value }));
  }

  updateAddImage(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.updateAddField('imagePath', val);
    this.imagePreview.set(val);
  }

  validateAddForm(): boolean {
    const f = this.addForm();
    if (!f.name.trim())        { toast.error('Product name is required');       return false; }
    if (!f.categoryId)         { toast.error('Please select a category');       return false; }
    if (f.quantity <= 0)       { toast.error('Quantity must be greater than 0'); return false; }
    if (f.price <= 0)          { toast.error('Price must be greater than 0');   return false; }
    if (!f.imagePath.trim())   { toast.error('Image URL is required');          return false; }
    if (!f.description.trim()) { toast.error('Description is required');        return false; }
    return true;
  }

  addProduct(): void {
    if (!this.validateAddForm()) return;

    this.productService.addProduct(this.addForm()).subscribe({
      next: (response: ApiResponse<AddNewProductResponseDTO>) => {
        const category = this.store.value.categories.find(
          c => c.categoryId === this.addForm().categoryId,
        );
        const newProduct: ProductDetails = {
          productId:    response.data?.productId ?? Date.now().toString(),
          stockId:      '',
          categoryId:   this.addForm().categoryId,
          categoryName: category?.categoryName ?? '',
          productName:  this.addForm().name,
          imagePath:    this.addForm().imagePath,
          description:  this.addForm().description,
          price:        this.addForm().price,
          quantity:     this.addForm().quantity,
          review:       [],
        };
        this.store.addProduct(newProduct);
        toast.success('Product added successfully!');
        this.resetAddForm();
        this.activeView.set('list');
      },
      error: err => toast.error(err?.error?.message || 'Failed to add product'),
    });
  }

  resetAddForm(): void {
    this.addForm.set({ categoryId: '', name: '', imagePath: '', description: '', price: 0, quantity: 0 });
    this.imagePreview.set('');
  }

  // ── Edit ──────────────────────────────────────────────────────────────────
  openEditModal(product: ProductDetails): void {
    this.editingProduct.set(product);
    this.editForm.set({
      productId:   product.productId,
      categoryId:  product.categoryId,
      name:        product.productName,
      imagePath:   product.imagePath,
      description: product.description,
      price:       product.price,
      quantity:    product.quantity,
    });
    this.editImagePreview.set(product.imagePath);
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.editingProduct.set(null);
    this.editImagePreview.set('');
  }

  updateEditImage(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.editForm.update(f => ({ ...f, imagePath: val }));
    this.editImagePreview.set(val);
  }

  saveProduct(): void {
    const f = this.editForm();
    if (!f.name.trim() || !f.categoryId || f.price <= 0) {
      toast.error('Please fill all required fields correctly');
      return;
    }

    this.productService.updateProduct(f).subscribe({
      next: (response: ApiResponse<UpdateProductResponseDTO>) => {
        if (response.data?.isUpdate) {
          const category = this.store.value.categories.find(c => c.categoryId === f.categoryId);
          const existing = this.store.value.products.find(p => p.productId === f.productId);
          this.store.updateProduct({
            productId:    f.productId,
            stockId:      existing?.stockId ?? '',
            categoryId:   f.categoryId,
            categoryName: category?.categoryName ?? '',
            productName:  f.name,
            imagePath:    f.imagePath,
            description:  f.description,
            price:        f.price,
            quantity:     f.quantity,
            review:       existing?.review ?? [],
          });
          toast.success('Product updated successfully!');
          this.closeEditModal();
        } else {
          toast.error('Failed to update product');
        }
      },
      error: err => toast.error(err?.error?.message || 'Failed to update product'),
    });
  }

  // ── Delete ────────────────────────────────────────────────────────────────
  openDeleteModal(productId: string): void {
    this.pendingDeleteId.set(productId);
    this.confirmModal.set(true);
  }

  cancelDelete(): void {
    this.pendingDeleteId.set('');
    this.confirmModal.set(false);
  }

  // Removes from store only — wire your API call here when ready
  deleteProduct(): void {
    const id = this.pendingDeleteId();
    if (!id) return;
    this.store.setProducts(this.store.value.products.filter(p => p.productId !== id));
    toast.success('Product deleted successfully');
    this.cancelDelete();
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  avgRating(reviews: ReviewDTO[]): string {
    if (!reviews?.length) return '—';
    const avg = reviews.reduce((s, r) => s + r.reviewPoints, 0) / reviews.length;
    return avg.toFixed(1);
  }
}