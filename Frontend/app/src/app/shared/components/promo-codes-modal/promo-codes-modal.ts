import { Component, signal, output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PromoCode {
  id: number;
  code: string;
  title: string;
  discount: string;
  discountType: 'percent' | 'flat';
  color: string;
}

@Component({
  selector: 'app-promo-codes-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './promo-codes-modal.html',
  styleUrls: ['./promo-codes-modal.css'],
})
export class PromoCodesModalComponent {
  close = output<void>();

  copiedCode = signal<string | null>(null);

  promoCodes: PromoCode[] = [
    { id: 1,  code: 'WELCOME20', title: 'Welcome Offer',   discount: '20% OFF',        discountType: 'percent', color: '#4F46E5' },
    { id: 2,  code: 'FLAT100',   title: 'Flat Discount',   discount: '₹100 OFF',       discountType: 'flat',    color: '#059669' },
    { id: 3,  code: 'SUMMER30',  title: 'Summer Sale',     discount: '30% OFF',        discountType: 'percent', color: '#D97706' },
    { id: 4,  code: 'FREESHIP',  title: 'Free Shipping',   discount: 'FREE DELIVERY',  discountType: 'flat',    color: '#7C3AED' },
    { id: 5,  code: 'SAVE50',    title: 'Big Savings',     discount: '₹50 OFF',        discountType: 'flat',    color: '#DC2626' },
    { id: 6,  code: 'FLASH15',   title: 'Flash Sale',      discount: '15% OFF',        discountType: 'percent', color: '#0891B2' },
    { id: 7,  code: 'MEGA40',    title: 'Mega Deal',       discount: '40% OFF',        discountType: 'percent', color: '#BE185D' },
    { id: 8,  code: 'EXTRA200',  title: 'Extra Savings',   discount: '₹200 OFF',       discountType: 'flat',    color: '#065F46' },
    { id: 9,  code: 'FESTIVE25', title: 'Festive Special', discount: '25% OFF',        discountType: 'percent', color: '#92400E' },
    { id: 10, code: 'NEWUSER10', title: 'New User',        discount: '10% OFF',        discountType: 'percent', color: '#1D4ED8' },
    { id: 11, code: 'WEEKEND35', title: 'Weekend Offer',   discount: '35% OFF',        discountType: 'percent', color: '#6D28D9' },
    { id: 12, code: 'FLAT500',   title: 'Super Saver',     discount: '₹500 OFF',       discountType: 'flat',    color: '#B45309' },
  ];

  copyCode(code: string): void {
    navigator.clipboard.writeText(code).then(() => {
      this.copiedCode.set(code);
      setTimeout(() => this.copiedCode.set(null), 2000);
    });
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('promo-modal__backdrop')) {
      this.close.emit();
    }
  }
}
