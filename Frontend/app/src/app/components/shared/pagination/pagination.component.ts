import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { getPageRange } from '../../../constants/pagination.constants';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.css',
})
export class PaginationComponent {
  @Input() currentPage = 1;
  @Input() totalPages = 1;
  @Input() totalItems = 0;
  @Input() pageSize = 10;
  @Input() isLoading = false;
  @Input() maxVisiblePages = 5;

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  get pageNumbers(): number[] {
    return getPageRange(this.currentPage, this.totalPages, this.maxVisiblePages);
  }

  get startItem(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }

  get canGoPrevious(): boolean {
    return this.currentPage > 1 && !this.isLoading;
  }

  get canGoNext(): boolean {
    return this.currentPage < this.totalPages && !this.isLoading;
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage && !this.isLoading) {
      this.pageChange.emit(page);
    }
  }

  previousPage(): void {
    if (this.canGoPrevious) {
      this.pageChange.emit(this.currentPage - 1);
    }
  }

  nextPage(): void {
    if (this.canGoNext) {
      this.pageChange.emit(this.currentPage + 1);
    }
  }

  firstPage(): void {
    if (this.currentPage !== 1 && !this.isLoading) {
      this.pageChange.emit(1);
    }
  }

  lastPage(): void {
    if (this.currentPage !== this.totalPages && !this.isLoading) {
      this.pageChange.emit(this.totalPages);
    }
  }
}
