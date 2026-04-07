import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { map } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { toast } from 'ngx-sonner';
import { StoreService } from '../../services/store.service';
import { AdminProductService } from '../../services/products.service';
import { ProductLimitService } from '../../services/product-limit.service';
import { PaginationModel } from '../../../../shared/models/admin/pagination.model';
import { ApiResponse } from '../../../../shared/models/admin/apiResponse.model';
import { GetAllProductsResponseDTO } from '../../../../shared/models/admin/products.model';
import {
  UserMonthlyProductLimitDTO,
  GetAllLimitsResponseDTO,
  AddLimitRequestDTO,
  EditLimitRequestDTO,
} from '../../../../shared/models/admin/product-limit.model';

@Component({
  selector: 'app-product-limit',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, AsyncPipe],
  templateUrl: './product-limit.html',
  styleUrl: './product-limit.css',
})
export class ProductLimit implements OnInit {
  private fb = inject(FormBuilder);
  private storeService = inject(StoreService);
  private productService = inject(AdminProductService);
  private limitService = inject(ProductLimitService);

  products$ = this.storeService.state$.pipe(map((s) => s.products));

  records = signal<UserMonthlyProductLimitDTO[]>([]);
  totalCount = signal<number>(0);
  currentPage = signal<number>(1);
  readonly pageSize = 10;
  isLoadingTable = signal<boolean>(false);
  hasMoreData = signal<boolean>(true);

  // Form state
  form!: FormGroup;
  isLoadingProducts = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  editingId = signal<string | null>(null);

  // Delete modal
  deleteModal = signal<boolean>(false);
  pendingDeleteId = signal<string>('');

  ngOnInit(): void {
    this.form = this.fb.group({
      productId: ['', Validators.required],
      monthlyLimit: [null, [Validators.required, Validators.min(1)]],
    });

    if (this.storeService.value.products.length === 0) {
      this.loadProducts();
    }

    this.fetchPage(1);
  }

  private loadProducts(): void {
    this.isLoadingProducts.set(true);
    const pagination = new PaginationModel();
    pagination.pageNumber = 1;
    pagination.pageSize = 100;

    this.productService.getAllProducts(pagination).subscribe({
      next: (response: ApiResponse<GetAllProductsResponseDTO>) => {
        this.storeService.setProducts(response.data?.productList ?? []);
        this.isLoadingProducts.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to load products');
        this.isLoadingProducts.set(false);
      },
    });
  }

  private fetchPage(page: number): void {
    this.isLoadingTable.set(true);
    const pagination = new PaginationModel();
    pagination.pageNumber = page;
    pagination.pageSize = this.pageSize;

    this.limitService.getAllLimits(pagination).subscribe({
      next: (response: ApiResponse<GetAllLimitsResponseDTO>) => {
        const incoming = response.data?.records ?? [];
        this.totalCount.set(response.data?.totalCount ?? 0);

        if (page === 1) {
          this.records.set(incoming);
        } else {
          this.records.update((prev) => {
            const existingIds = new Set(prev.map((r) => r.id));
            return [...prev, ...incoming.filter((r) => !existingIds.has(r.id))];
          });
        }

        this.currentPage.set(page);
        this.hasMoreData.set(incoming.length === this.pageSize);
        this.isLoadingTable.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to load records');
        this.isLoadingTable.set(false);
      },
    });
  }

  get pagedRecords(): UserMonthlyProductLimitDTO[] {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.records().slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  getPageNumbers(): number[] {
    const total = this.totalPages;
    const current = this.currentPage();
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    const range: number[] = [];
    for (let i = start; i <= end; i++) range.push(i);
    return range;
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || this.isLoadingTable()) return;
    const alreadyFetched = this.records().length >= page * this.pageSize;
    if (alreadyFetched) {
      this.currentPage.set(page);
    } else {
      this.fetchPage(page);
    }
  }

  prevPage(): void {
    if (this.currentPage() > 1) this.goToPage(this.currentPage() - 1);
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages) this.goToPage(this.currentPage() + 1);
  }

  // ── Add / Edit ─────────────────────────────────────────────────────────

  onSubmit(): void {
    if (this.form.invalid) return;
    this.isSubmitting.set(true);

    if (this.editingId()) {
      this.submitEdit();
    } else {
      this.submitAdd();
    }
  }

  private submitAdd(): void {
    const payload: AddLimitRequestDTO = {
      productId: this.form.value.productId,
      monthlyLimit: this.form.value.monthlyLimit,
    };

    this.limitService.addLimit(payload).subscribe({
      next: (response) => {
        toast.success(response.message || 'Limit added successfully');
        this.resetForm();
        this.refreshTable();
        this.isSubmitting.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to add limit');
        this.isSubmitting.set(false);
      },
    });
  }

  private submitEdit(): void {
    const payload: EditLimitRequestDTO = {
      id: this.editingId()!,
      monthlyLimit: this.form.value.monthlyLimit,
    };

    this.limitService.editLimit(payload).subscribe({
      next: (response) => {
        toast.success(response.message || 'Limit updated successfully');
        this.records.update((prev) =>
          prev.map((r) =>
            r.id === payload.id ? { ...r, monthlyLimit: payload.monthlyLimit } : r,
          ),
        );
        this.resetForm();
        this.isSubmitting.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to update limit');
        this.isSubmitting.set(false);
      },
    });
  }

  startEdit(record: UserMonthlyProductLimitDTO): void {
    this.editingId.set(record.id);
    this.form.patchValue({
      productId: record.productId,
      monthlyLimit: record.monthlyLimit,
    });
    this.form.get('productId')?.disable();
  }

  cancelEdit(): void {
    this.resetForm();
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.form.reset({ productId: '', monthlyLimit: null });
    this.form.get('productId')?.enable();
  }

  // ── Delete ─────────────────────────────────────────────────────────────

  openDeleteModal(id: string): void {
    this.pendingDeleteId.set(id);
    this.deleteModal.set(true);
  }

  cancelDelete(): void {
    this.deleteModal.set(false);
    this.pendingDeleteId.set('');
  }

  confirmDelete(): void {
    const id = this.pendingDeleteId();
    if (!id) return;

    this.limitService.deleteLimit({ id }).subscribe({
      next: (response) => {
        toast.success(response.message || 'Limit deleted successfully');
        this.records.update((prev) => prev.filter((r) => r.id !== id));
        this.totalCount.update((c) => c - 1);
        this.cancelDelete();
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to delete limit');
        this.cancelDelete();
      },
    });
  }

  private refreshTable(): void {
    this.records.set([]);
    this.fetchPage(1);
  }
}
