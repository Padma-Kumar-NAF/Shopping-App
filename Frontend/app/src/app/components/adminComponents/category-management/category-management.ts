import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toast } from 'ngx-sonner';
import { PaginationModel } from '../../../models/users/pagination.model';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { ApiResponse } from '../../../models/users/apiResponse.model';
import {
  CategoryDTO,
  DeleteCategoryResponseDTO,
  AddCategoryResponseDTO,
  EditCategoryResponseDTO,
} from '../../../models/admin/categories.model';
import { StoreService } from '../../../services/adminServices/store.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-management.html',
  styleUrl: './category-management.css',
})
export class CategoryManagement implements OnInit {
  apiService = inject(AdminCategoryService);
  store = inject(StoreService);

  categories$!: Observable<CategoryDTO[]>;

  pagination: PaginationModel;

  // Add form
  newCategory = signal<string>('');
  showAddForm = signal<boolean>(false);

  // Edit modal
  showEditModal = signal<boolean>(false);
  editingCategory = signal<CategoryDTO | null>(null);
  editForm = signal<{ name: string }>({ name: '' });

  // Delete confirm modal
  confirmModal = signal<boolean>(false);
  deleteCategoryId = signal<string>('');

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = 10;
  }

  ngOnInit(): void {
    this.categories$ = this.store.state$.pipe(map(s => s.categories));
  }

  // ── Add ──────────────────────────────────────────────────────────────────

  toggleAddForm(): void {
    this.showAddForm.update(v => !v);
    if (!this.showAddForm()) this.resetForm();
  }

  addCategory(): void {
    const name = this.newCategory().trim();

    if (!name) {
      toast.error('Category name is required');
      return;
    }

    const exists = this.store.value.categories.some(
      c => c.categoryName.toLowerCase() === name.toLowerCase()
    );
    if (exists) {
      toast.error('Category already exists');
      return;
    }

    this.apiService.addCategory(name).subscribe({
      next: (response: ApiResponse<AddCategoryResponseDTO>) => {
        const newCat: CategoryDTO = {
          categoryId: response.data?.categoryId ?? Date.now().toString(),
          categoryName: name,
          productsCount: 0,
          createdAt: new Date(),
        };
        this.store.addCategory(newCat);
        toast.success('Category added successfully!');
        this.resetForm();
        this.showAddForm.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Something went wrong');
      },
    });
  }

  resetForm(): void {
    this.newCategory.set('');
  }

  // ── Edit ─────────────────────────────────────────────────────────────────

  openEditModal(category: CategoryDTO): void {
    this.editingCategory.set(category);
    this.editForm.set({ name: category.categoryName });
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.editingCategory.set(null);
  }

  saveCategory(): void {
    const category = this.editingCategory();
    if (!category) return;

    const name = this.editForm().name.trim();

    if (!name) {
      toast.error('Category name is required');
      return;
    }

    const exists = this.store.value.categories.some(
      c => c.categoryId !== category.categoryId &&
           c.categoryName.toLowerCase() === name.toLowerCase()
    );
    if (exists) {
      toast.error('Category name already exists');
      return;
    }

    this.apiService.updateCategory(category.categoryId, name).subscribe({
      next: (response: ApiResponse<EditCategoryResponseDTO>) => {
        if (response.data?.isSuccess) {
          this.store.updateCategory({ ...category, categoryName: name });
          toast.success('Category updated successfully!');
          this.closeEditModal();
        } else {
          toast.error('Failed to update category');
        }
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Something went wrong');
      },
    });
  }

  // ── Delete ───────────────────────────────────────────────────────────────

  getDeleteCategoryId(categoryId: string): void {
    this.deleteCategoryId.set(categoryId);
    this.confirmModal.set(true);
  }

  cancelDelete(): void {
    this.deleteCategoryId.set('');
    this.confirmModal.set(false);
  }

  deleteCategory(): void {
    const id = this.deleteCategoryId();

    this.apiService.deleteCategory(id).subscribe({
      next: (response: ApiResponse<DeleteCategoryResponseDTO>) => {
        if (response.data?.isSuccess) {
          const updated = this.store.value.categories.filter(c => c.categoryId !== id);
          this.store.setCategories(updated);
          toast.success(response.message || 'Category deleted successfully');
        } else {
          toast.error('Failed to delete category');
        }
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Something went wrong');
      },
      complete: () => {
        this.deleteCategoryId.set('');
        this.confirmModal.set(false);
      },
    });
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  totalProducts(categories: CategoryDTO[]): number {
    return categories.reduce((sum, cat) => sum + (cat.productsCount || 0), 0);
  }

  avgProducts(categories: CategoryDTO[]): string {
    if (!categories.length) return '0';
    return (this.totalProducts(categories) / categories.length).toFixed(1);
  }
}