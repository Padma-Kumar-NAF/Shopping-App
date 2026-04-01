import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toast } from 'ngx-sonner';
import { PaginationModel } from '../../../../shared/models/users/pagination.model';
import { AdminCategoryService } from '../../services/category.service';
import { ApiResponse } from '../../../../shared/models/users/apiResponse.model';
import {
  CategoryDTO,
  DeleteCategoryResponseDTO,
  AddCategoryResponseDTO,
  EditCategoryResponseDTO,
  GetAllCategoryResponseDTO,
} from '../../../../shared/models/admin/categories.model';
import { StoreService } from '../../services/store.service';
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

  newCategory = signal<string>('');
  showAddForm = signal<boolean>(false);

  showEditModal = signal<boolean>(false);
  editingCategory = signal<CategoryDTO | null>(null);
  editForm = signal<{ name: string }>({ name: '' });

  confirmModal = signal<boolean>(false);
  deleteCategoryId = signal<string>('');

  // ── Pagination ────────────────────────────────────────────────────────────
  currentPage = signal<number>(1);
  readonly pageSize = 10;

  hasMoreData = signal<boolean>(true);
  isLoading = signal<boolean>(false);

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = this.pageSize;
  }

  ngOnInit(): void {
    this.categories$ = this.store.state$.pipe(map(s => s.categories));

    if (this.store.value.categories.length === 0) {
      this.fetchPage(1);
    }
  }

  // ── Paged getter ──────────────────────────────────────────────────────────

  get pagedCategories(): CategoryDTO[] {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.store.value.categories.slice(start, start + this.pageSize);
  }

  // ── Pagination helpers ────────────────────────────────────────────────────

  get totalCategories(): number {
    return this.store.value.categories.length;
  }

  get totalPages(): number {
    const fromStore = Math.max(1, Math.ceil(this.totalCategories / this.pageSize));
    return this.hasMoreData() ? fromStore + 1 : fromStore;
  }

  getPageNumbers(): number[] {
    const total = Math.max(1, Math.ceil(this.totalCategories / this.pageSize));
    const current = this.currentPage();
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    const range: number[] = [];
    for (let i = start; i <= end; i++) range.push(i);
    return range;
  }

  goToPage(page: number): void {
    if (page < 1 || this.isLoading()) return;

    const alreadyFetched = this.store.pageCache.categories.has(page);
    const dataExistsForPage =
      this.store.value.categories.length >= page * this.pageSize || alreadyFetched;

    if (dataExistsForPage) {
      this.currentPage.set(page);
      return;
    }

    this.fetchPage(page);
  }

  prevPage(): void {
    if (this.currentPage() <= 1 || this.isLoading()) return;
    this.currentPage.update(p => p - 1);
  }

  nextPage(): void {
    if (this.isLoading()) return;
    this.goToPage(this.currentPage() + 1);
  }

  // ── API fetch ─────────────────────────────────────────────────────────────

  private fetchPage(page: number): void {
    this.isLoading.set(true);
    this.pagination.pageNumber = page;
    this.pagination.pageSize = this.pageSize;

    this.apiService.getAllCategories(this.pagination).subscribe({
      next: (response: ApiResponse<GetAllCategoryResponseDTO>) => {
        const incoming = response.data?.categoryList ?? [];

        if (incoming.length === 0) {
          this.hasMoreData.set(false);
          toast.info('No more categories to load');
        } else {
          this.store.appendCategories(incoming);
          this.store.pageCache.categories.add(page);
          this.currentPage.set(page);

          if (incoming.length < this.pageSize) {
            this.hasMoreData.set(false);
          }
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        toast.error(err?.error?.message || 'Failed to load categories');
        this.isLoading.set(false);
      },
    });
  }

  // ── Add ───────────────────────────────────────────────────────────────────

  toggleAddForm(): void {
    this.showAddForm.update(v => !v);
    if (!this.showAddForm()) this.resetForm();
  }

  addCategory(): void {
    const name = this.newCategory().trim();
    if (!name) { toast.error('Category name is required'); return; }
    const exists = this.store.value.categories.some(
      c => c.categoryName.toLowerCase() === name.toLowerCase()
    );
    if (exists) { toast.error('Category already exists'); return; }

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
      error: err => toast.error(err?.error?.message || 'Something went wrong'),
    });
  }

  resetForm(): void { this.newCategory.set(''); }

  // ── Edit ──────────────────────────────────────────────────────────────────

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
    if (!name) { toast.error('Category name is required'); return; }
    const exists = this.store.value.categories.some(
      c => c.categoryId !== category.categoryId &&
           c.categoryName.toLowerCase() === name.toLowerCase()
    );
    if (exists) { toast.error('Category name already exists'); return; }

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
      error: err => toast.error(err?.error?.message || 'Something went wrong'),
    });
  }

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
          this.store.setCategories(this.store.value.categories.filter(c => c.categoryId !== id));
          toast.success(response.message || 'Category deleted successfully');
        } else {
          toast.error('Failed to delete category');
        }
      },
      error: err => toast.error(err?.error?.message || 'Something went wrong'),
      complete: () => {
        this.deleteCategoryId.set('');
        this.confirmModal.set(false);
      },
    });
  }

  totalProducts(categories: CategoryDTO[]): number {
    return categories.reduce((sum, cat) => sum + (cat.productsCount || 0), 0);
  }

  avgProducts(categories: CategoryDTO[]): string {
    if (!categories.length) return '0';
    return (this.totalProducts(categories) / categories.length).toFixed(1);
  }
}