import { Component, signal, output, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PromoCodeService } from '../../../features/admin/services/promocode.service';
import { GetAllUserPromoCodesResponseDTO, UserPromoCodeItemDTO } from '../../models/users/promoCode.model';
import { ApiResponse } from '../../models/users/apiResponse.model';
import { AuthStateService } from '../../../core/state/auth-state.service';

const CARD_COLORS = [
  '#4F46E5', '#059669', '#D97706', '#7C3AED',
  '#DC2626', '#0891B2', '#BE185D', '#065F46',
  '#92400E', '#1D4ED8', '#6D28D9', '#B45309',
];

@Component({
  selector: 'app-promo-codes-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './promo-codes-modal.html',
  styleUrls: ['./promo-codes-modal.css'],
})
export class PromoCodesModalComponent implements OnInit {
  private apiService = inject(PromoCodeService);
  private authState = inject(AuthStateService);

  close = output<void>();
  isLoading = signal(true);
  promoCodes = signal<UserPromoCodeItemDTO[]>([]);
  copiedCode = signal<string | null>(null);

  ngOnInit(): void {
    if (this.authState.isAuthenticated()) {
      this.apiService.getAllUserPromo().subscribe({
        next: (response: ApiResponse<GetAllUserPromoCodesResponseDTO>) => {
          this.promoCodes.set(response.data?.promoCodes ?? []);
        },
        error: (err) => {
          console.error(err);
          this.isLoading.set(false);
        },
        complete: () => {
          this.isLoading.set(false);
        }
      });
    }
    else {
      this.isLoading.set(false)
    }
  }

  skeletonItems = Array(8);

  getColor(index: number): string {
    return CARD_COLORS[index % CARD_COLORS.length];
  }

  copyCode(id: string, code: string): void {
    navigator.clipboard.writeText(code).then(() => {
      this.copiedCode.set(id);
      setTimeout(() => this.copiedCode.set(null), 2000);
    });
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('promo-modal__backdrop')) {
      this.close.emit();
    }
  }
}
