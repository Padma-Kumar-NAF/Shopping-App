import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { PromoCodeService } from '../../../services/adminServices/promocode.service';
import { AddPromoCodeRequestDTO } from '../../../models/admin/promocode.model';
import { toast } from 'ngx-sonner';

function notPastDateValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return new Date(control.value) < today ? { pastDate: true } : null;
}

function toDateAfterFromValidator(group: AbstractControl): ValidationErrors | null {
  const from = group.get('fromDate')?.value;
  const to = group.get('toDate')?.value;
  if (from && to && new Date(to) < new Date(from)) {
    return { toBeforeFrom: true };
  }
  return null;
}

@Component({
  selector: 'app-promocode-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './promocode-management.html',
  styleUrl: './promocode-management.css',
})
export class PromocodeManagement {
  private promoCodeService = inject(PromoCodeService);

  today = new Date().toISOString().split('T')[0];
  submitted = signal(false);
  isLoading = signal(false);

  form = new FormGroup(
    {
      promoCode: new FormControl('', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(20),
        Validators.pattern(/^[A-Z0-9]+$/),
      ]),
      discountPercent: new FormControl<number | null>(null, [
        Validators.required,
        Validators.min(1),
        Validators.max(100),
      ]),
      fromDate: new FormControl('', [Validators.required, notPastDateValidator]),
      toDate: new FormControl('', [Validators.required, notPastDateValidator]),
    },
    { validators: toDateAfterFromValidator }
  );

  get promoCode() { return this.form.get('promoCode')!; }
  get discountPercent() { return this.form.get('discountPercent')!; }
  get fromDate() { return this.form.get('fromDate')!; }
  get toDate() { return this.form.get('toDate')!; }

  onSubmit(): void {
    this.submitted.set(true);
    if (this.form.invalid) return;

    const request: AddPromoCodeRequestDTO = {
      PromoCodeName: this.promoCode.value!,
      discountPercentage: this.discountPercent.value!,
      fromDate: this.fromDate.value!,
      toDate: this.toDate.value!,
    };

    this.isLoading.set(true);
    const toastId = toast.loading('Creating promo code...');

    this.promoCodeService.addPromoCode(request).subscribe({
      next: (res) => {
        toast.dismiss(toastId);
        if (res.data?.isSuccess) {
          toast.success('Promo code created successfully');
          this.reset();
        } else {
          toast.error(res.message || 'Failed to create promo code');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.dismiss(toastId);
        toast.error(err?.error?.message || 'Failed to create promo code');
        this.isLoading.set(false);
      },
    });
  }

  reset(): void {
    this.form.reset();
    this.submitted.set(false);
  }
}
