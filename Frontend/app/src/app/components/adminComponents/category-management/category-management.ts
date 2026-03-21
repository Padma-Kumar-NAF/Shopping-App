import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category } from '../../../models/admin.model';
import { toast } from 'ngx-sonner';
import { LoaderService } from '../../../services/loading.service';
import { PaginationModel } from '../../../models/pagination.model';
import { AdminCategoryService } from '../../../services/adminServices/category.service';
import { ApiResponse } from '../../../models/apiResponse.model';
import {
  CategoryDTO,
  DeleteCategoryResponseDTO,
  GetAllCategoryResponseDTO,
  AddCategoryResponseDTO,
  EditCategoryResponseDTO,
} from '../../../models/admin/categories.model';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-management.html',
  styleUrl: './category-management.css',
})
export class CategoryManagement implements OnInit {
  // loader = inject(LoaderService);
  apiService: AdminCategoryService = inject(AdminCategoryService);

  pagination: PaginationModel;
  deleteCategoryId = signal<string>('');
  confirmModal = signal<boolean>(false);

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = 10;
  }

  ngOnInit(): void {
    this.getAllCategories();
  }

  getAllCategories() {
    // this.loader.show();
    this.apiService.getAllCategories(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllCategoryResponseDTO>) => {
        console.log('response', response);
        this.categories.set(response.data?.categoryList ?? []);
      },
      error: (err) => {
        console.error(err);
        toast.error('Failed to load categories');
      },
      complete: () => {
        // this.loader.hide();
      },
    });
  }

  categories = signal<CategoryDTO[]>([]);
  newCategory = signal<string>('');
  showAddForm = signal<boolean>(false);
  showEditModal = signal<boolean>(false);
  editingCategory = signal<CategoryDTO | null>(null);
  editForm = signal<{ name: string }>({ name: '' });

  toggleAddForm(): void {
    this.showAddForm.update((v) => !v);
    if (!this.showAddForm()) {
      this.resetForm();
    }
  }

  addCategory(): void {
    const cat = this.newCategory();

    if (!cat.trim()) {
      toast.error('Category name is required');
      return;
    }

    const exists = this.categories().some(
      (c) => c.categoryName.toLowerCase() === cat.trim().toLowerCase()
    );

    if (exists) {
      toast.error('Category already exists');
      return;
    }

    // this.loader.show();
    this.apiService.addCategory(cat.trim()).subscribe({
      next: (response: ApiResponse<AddCategoryResponseDTO>) => {
        const newCat: CategoryDTO = {
          categoryId: response.data?.categoryId ?? Date.now().toString(),
          categoryName: cat.trim(),
          productsCount: 0,
          createdAt: new Date(),
        };
        this.categories.update((cats) => [...cats, newCat]);
        toast.success('Category added successfully!');
        this.resetForm();
        this.showAddForm.set(false);
      },
      error: (err) => {
        console.error(err);
        const errorMessage = err?.error?.message || 'Something went wrong';
        toast.error(errorMessage);
      },
      complete: () => {
        // this.loader.hide();
      },
    });
  }

  getDeleteCategoryId(categoryId: string): void {
    this.deleteCategoryId.set(categoryId);
    this.confirmModal.set(true);
  }

  deleteCategory(): void {
    const id = this.deleteCategoryId();

    // this.loader.show();
    this.apiService.deleteCategory(id).subscribe({
      next: (response: ApiResponse<DeleteCategoryResponseDTO>) => {
        if (response.data?.isSuccess) {
          this.categories.update((cats) =>
            cats.filter((c) => c.categoryId !== id)
          );
          toast.success(response.message || 'Category deleted successfully');
        } else {
          toast.error('Failed to delete category');
        }
      },
      error: (err) => {
        console.error(err);
        const errorMessage = err?.error?.message || 'Something went wrong';
        toast.error(errorMessage);
      },
      complete: () => {
        // this.loader.hide();
        this.deleteCategoryId.set('');
        this.confirmModal.set(false);
      },
    });
  }

  cancelDelete(): void {
    this.deleteCategoryId.set('');
    this.confirmModal.set(false);
  }

  resetForm(): void {
    this.newCategory.set('');
  }

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

    const form = this.editForm();

    if (!form.name.trim()) {
      toast.error('Category name is required');
      return;
    }

    const exists = this.categories().some(
      (c) =>
        c.categoryId !== category.categoryId &&
        c.categoryName.toLowerCase() === form.name.trim().toLowerCase()
    );

    if (exists) {
      toast.error('Category name already exists');
      return;
    }

    // this.loader.show();
    this.apiService.updateCategory(category.categoryId, form.name.trim()).subscribe({
      next: (response: ApiResponse<EditCategoryResponseDTO>) => {
        if (response.data?.isSuccess) {
          this.categories.update((cats) =>
            cats.map((c) =>
              c.categoryId === category.categoryId
                ? { ...c, categoryName: form.name.trim() }
                : c
            )
          );
          toast.success('Category updated successfully!');
          this.closeEditModal();
        } else {
          toast.error('Failed to update category');
        }
      },
      error: (err) => {
        console.error(err);
        const errorMessage = err?.error?.message || 'Something went wrong';
        toast.error(errorMessage);
      },
      complete: () => {
        // this.loader.hide();
      },
    });
  }
}