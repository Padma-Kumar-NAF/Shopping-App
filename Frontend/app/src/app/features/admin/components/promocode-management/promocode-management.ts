import { Component, inject, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, FormGroup, FormControl, Validators, AbstractControl, ValidationErrors } from "@angular/forms";
import { map } from "rxjs/operators";
import { Observable } from "rxjs";
import { toast } from "ngx-sonner";
import { PromoCodeService } from "../../services/promocode.service";
import { StoreService } from "../../services/store.service";
import { PromoCodeItemDTO, AddPromoCodeRequestDTO, EditPromoCodeRequestDTO } from "../../../../shared/models/admin/promocode.model";

function notPastDateValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return new Date(control.value) < today ? { pastDate: true } : null;
}

function toDateAfterFromValidator(group: AbstractControl): ValidationErrors | null {
  const from = group.get("fromDate")?.value;
  const to = group.get("toDate")?.value;
  if (from && to && new Date(to) < new Date(from)) return { toBeforeFrom: true };
  return null;
}

@Component({
  selector: "app-promocode-management",
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: "./promocode-management.html",
  styleUrl: "./promocode-management.css",
})
export class PromocodeManagement implements OnInit {
  private promoService = inject(PromoCodeService);
  store = inject(StoreService);

  promoCodes$!: Observable<PromoCodeItemDTO[]>;
  today = new Date().toISOString().split("T")[0];
  isLoading = signal(false);
  showAddForm = signal(false);
  submitted = signal(false);
  showEditModal = signal(false);
  editingPromo = signal<PromoCodeItemDTO | null>(null);
  editSubmitted = signal(false);
  confirmModal = signal(false);
  deletePromoId = signal("");
  currentPage = signal(1);
  readonly pageSize = 10;
  hasMoreData = signal(true);

  addForm = new FormGroup(
    {
      promoCode: new FormControl("", [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern(/^[A-Z0-9]+$/)]),
      discountPercent: new FormControl<number | null>(null, [Validators.required, Validators.min(1), Validators.max(100)]),
      fromDate: new FormControl("", [Validators.required, notPastDateValidator]),
      toDate: new FormControl("", [Validators.required, notPastDateValidator]),
    },
    { validators: toDateAfterFromValidator }
  );

  editForm = new FormGroup(
    {
      promoCode: new FormControl("", [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern(/^[A-Z0-9]+$/)]),
      discountPercent: new FormControl<number | null>(null, [Validators.required, Validators.min(1), Validators.max(100)]),
      fromDate: new FormControl("", [Validators.required]),
      toDate: new FormControl("", [Validators.required]),
    },
    { validators: toDateAfterFromValidator }
  );

  ngOnInit(): void {
    this.promoCodes$ = this.store.state$.pipe(map((s) => s.promoCodes));
    if (this.store.value.promoCodes.length === 0) {
      this.fetchPage(1);
    }
  }

  get pagedPromoCodes(): PromoCodeItemDTO[] {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.store.value.promoCodes.slice(start, start + this.pageSize);
  }

  get totalPromoCodes(): number {
    return this.store.value.promoCodes.length;
  }

  getPageNumbers(): number[] {
    const total = Math.max(1, Math.ceil(this.totalPromoCodes / this.pageSize));
    const current = this.currentPage();
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    const range: number[] = [];
    for (let i = start; i <= end; i++) range.push(i);
    return range;
  }

  goToPage(page: number): void {
    if (page < 1 || this.isLoading()) return;
    const alreadyFetched = this.store.pageCache.promoCodes.has(page);
    const dataExists = this.store.value.promoCodes.length >= page * this.pageSize || alreadyFetched;
    if (dataExists) { this.currentPage.set(page); return; }
    this.fetchPage(page);
  }

  prevPage(): void {
    if (this.currentPage() <= 1 || this.isLoading()) return;
    this.currentPage.update((p) => p - 1);
  }

  nextPage(): void {
    if (this.isLoading()) return;
    this.goToPage(this.currentPage() + 1);
  }

  fetchPage(page: number): void {
    this.isLoading.set(true);
    this.promoService.getAllPromoCodes(page, this.pageSize).subscribe({
      next: (res) => {
        const incoming = res.data?.promoCodes ?? [];
        if (incoming.length === 0) {
          this.hasMoreData.set(false);
          if (page > 1) toast.info("No more promo codes to load");
        } else {
          this.store.appendPromoCodes(incoming);
          this.store.pageCache.promoCodes.add(page);
          this.currentPage.set(page);
          if (incoming.length < this.pageSize) this.hasMoreData.set(false);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || "Failed to load promo codes");
        this.isLoading.set(false);
      },
    });
  }

  toggleAddForm(): void {
    this.showAddForm.update((v) => !v);
    if (!this.showAddForm()) this.resetAddForm();
  }

  onAddSubmit(): void {
    this.submitted.set(true);
    if (this.addForm.invalid) return;
    const request: AddPromoCodeRequestDTO = {
      promoCodeName: this.addForm.value.promoCode!,
      discountPercentage: this.addForm.value.discountPercent!,
      fromDate: this.addForm.value.fromDate!,
      toDate: this.addForm.value.toDate!,
    };
    this.isLoading.set(true);
    this.promoService.addPromoCode(request).subscribe({
      next: (res) => {
        const newPromo: PromoCodeItemDTO = {
          promoCodeId: res.data?.promoCodeId ?? "",
          promoCodeName: request.promoCodeName,
          discountPercentage: request.discountPercentage,
          fromDate: request.fromDate,
          toDate: request.toDate,
        };
        this.store.addPromoCode(newPromo);
        toast.success("Promo code created successfully");
        this.resetAddForm();
        this.showAddForm.set(false);
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || "Failed to create promo code");
        this.isLoading.set(false);
      },
    });
  }

  resetAddForm(): void {
    this.addForm.reset();
    this.submitted.set(false);
  }

  openEditModal(promo: PromoCodeItemDTO): void {
    this.editingPromo.set(promo);
    this.editForm.setValue({
      promoCode: promo.promoCodeName,
      discountPercent: promo.discountPercentage,
      fromDate: promo.fromDate.split("T")[0],
      toDate: promo.toDate.split("T")[0],
    });
    this.editSubmitted.set(false);
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.editingPromo.set(null);
    this.editForm.reset();
  }

  onEditSubmit(): void {
    this.editSubmitted.set(true);
    const promo = this.editingPromo();
    if (!promo || this.editForm.invalid) return;
    const request: EditPromoCodeRequestDTO = {
      promoCodeId: promo.promoCodeId,
      promoCodeName: this.editForm.value.promoCode!,
      discountPercentage: this.editForm.value.discountPercent!,
      fromDate: this.editForm.value.fromDate!,
      toDate: this.editForm.value.toDate!,
    };
    this.isLoading.set(true);
    this.promoService.editPromoCode(request).subscribe({
      next: (res) => {
        if (res.data?.isSuccess) {
          this.store.updatePromoCode({ ...promo, ...request });
          toast.success("Promo code updated successfully");
          this.closeEditModal();
        } else {
          toast.error("Failed to update promo code");
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || "Failed to update promo code");
        this.isLoading.set(false);
      },
    });
  }

  openDeleteConfirm(promoCodeId: string): void {
    this.deletePromoId.set(promoCodeId);
    this.confirmModal.set(true);
  }

  cancelDelete(): void {
    this.deletePromoId.set("");
    this.confirmModal.set(false);
  }

  confirmDelete(): void {
    const id = this.deletePromoId();
    this.isLoading.set(true);
    this.promoService.deletePromoCode({ promoCodeId: id }).subscribe({
      next: (res) => {
        if (res.data?.isSuccess) {
          this.store.removePromoCode(id);
          toast.success("Promo code deleted successfully");
        } else {
          toast.error("Failed to delete promo code");
        }
        this.cancelDelete();
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || "Failed to delete promo code");
        this.cancelDelete();
        this.isLoading.set(false);
      },
    });
  }

  isExpired(toDate: string): boolean {
    return new Date(toDate) < new Date();
  }

  isActive(fromDate: string, toDate: string): boolean {
    const now = new Date();
    return new Date(fromDate) <= now && now <= new Date(toDate);
  }

  activeCount(codes: PromoCodeItemDTO[]): number {
    return codes.filter((c) => this.isActive(c.fromDate, c.toDate)).length;
  }

  expiredCount(codes: PromoCodeItemDTO[]): number {
    return codes.filter((c) => this.isExpired(c.toDate)).length;
  }
}
