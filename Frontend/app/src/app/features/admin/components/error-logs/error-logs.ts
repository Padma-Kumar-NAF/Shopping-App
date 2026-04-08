import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorLogsService, ErrorLogDTO } from '../../services/error-logs.service';
import { PaginationModel } from '../../../../shared/models/users/pagination.model';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { DEFAULT_PAGE_SIZE, calculateTotalPages } from '../../../../constants/pagination.constants';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-error-logs',
  standalone: true,
  imports: [CommonModule, PaginationComponent],
  templateUrl: './error-logs.html',
  styleUrl: './error-logs.css',
})
export class ErrorLogs implements OnInit {
  private logsService = inject(ErrorLogsService);

  private allLogs = signal<ErrorLogDTO[]>([]);
  logs = signal<ErrorLogDTO[]>([]);
  isLoading = signal(false);
  currentPage = signal(1);
  totalItems = signal(0);
  totalPages = signal(0);
  pageSize = DEFAULT_PAGE_SIZE;

  // Sort: latest first by default
  sortAsc = signal(false);

  // Filter: 'all' | '5xx' | '4xx'
  activeFilter = signal<'all' | '5xx' | '4xx'>('all');

  // Detail popup
  selectedLog = signal<ErrorLogDTO | null>(null);

  openLog(log: ErrorLogDTO): void {
    this.selectedLog.set(log);
  }

  closeLog(): void {
    this.selectedLog.set(null);
  }

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    const pagination = new PaginationModel();
    pagination.pageNumber = this.currentPage();
    pagination.pageSize = this.pageSize;

    this.logsService.getErrorLogs(pagination).subscribe({
      next: (res) => {
        const items = res.data?.items ?? [];
        const sorted = [...items].sort((a, b) =>
          this.sortAsc()
            ? new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
            : new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.allLogs.set(sorted);
        this.applyFilter();
        this.totalItems.set(res.data?.totalCount ?? items.length);
        this.totalPages.set(calculateTotalPages(this.totalItems(), this.pageSize));
        this.isLoading.set(false);
      },
      error: (err) => {
        toast.error(err?.error?.message || 'Failed to load error logs');
        this.isLoading.set(false);
      },
    });
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadLogs();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  toggleSort(): void {
    this.sortAsc.update(v => !v);
    this.allLogs.update(items =>
      [...items].sort((a, b) =>
        this.sortAsc()
          ? new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
          : new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      )
    );
    this.applyFilter();
  }

  setFilter(filter: 'all' | '5xx' | '4xx'): void {
    this.activeFilter.set(filter);
    this.applyFilter();
  }

  private applyFilter(): void {
    const f = this.activeFilter();
    const all = this.allLogs();
    if (f === '5xx') {
      this.logs.set(all.filter(l => l.statusCode >= 500));
    } else if (f === '4xx') {
      this.logs.set(all.filter(l => l.statusCode >= 400 && l.statusCode < 500));
    } else {
      this.logs.set(all);
    }
  }

  statusRowClass(code: number): string {
    if (code >= 500) return 'bg-red-50 hover:bg-red-100';
    if (code >= 400) return 'bg-yellow-50 hover:bg-yellow-100';
    return 'hover:bg-gray-50';
  }

  statusBadgeClass(code: number): string {
    if (code >= 500) return 'bg-red-100 text-red-700 border-red-200';
    if (code >= 400) return 'bg-yellow-100 text-yellow-700 border-yellow-200';
    if (code >= 200) return 'bg-green-100 text-green-700 border-green-200';
    return 'bg-gray-100 text-gray-600 border-gray-200';
  }
}
