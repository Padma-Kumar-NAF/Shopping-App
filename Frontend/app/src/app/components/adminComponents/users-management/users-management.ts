import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toast } from 'ngx-sonner';
import { UserServcie } from '../../../services/adminServices/user.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { ApiResponse } from '../../../models/admin/apiResponse.model';
import { GetUsersResponseDTO, UserDetailsDTO } from '../../../models/admin/users.model';
import { StoreService } from '../../../services/adminServices/store.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-users-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './users-management.html',
  styleUrl: './users-management.css',
})
export class UsersManagement implements OnInit {
  private readonly apiService = inject(UserServcie);
  store = inject(StoreService);

  users$!: Observable<UserDetailsDTO[]>;
  filteredUsers$!: Observable<UserDetailsDTO[]>;

  confirmModal = signal<boolean>(false);
  pendingDeleteId = signal<string | null>(null);

  searchTerm = signal<string>('');
  filterRole = signal<string>('all');

  // ── Pagination ────────────────────────────────────────────────────────────
  currentPage = signal<number>(1);
  readonly pageSize = 10;

  /** Whether all pages from the API have been fetched */
  hasMoreData = signal<boolean>(true);

  /** Loading state for API calls */
  isLoading = signal<boolean>(false);

  pagination: PaginationModel;

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = this.pageSize;
  }

  ngOnInit(): void {
    this.users$ = this.store.state$.pipe(map(s => s.users));

    this.filteredUsers$ = this.store.state$.pipe(
      map(s => this.applyFilters(s.users))
    );
  }

  // ── Filter helpers ────────────────────────────────────────────────────────

  private applyFilters(users: UserDetailsDTO[]): UserDetailsDTO[] {
    let filtered = users;
    const term = this.searchTerm().toLowerCase();
    if (term) {
      filtered = filtered.filter(
        u => u.name.toLowerCase().includes(term) || u.email.toLowerCase().includes(term)
      );
    }
    if (this.filterRole() !== 'all') {
      filtered = filtered.filter(u => u.role === this.filterRole());
    }
    return filtered;
  }

  // ── Paged getter ──────────────────────────────────────────────────────────

  get pagedUsers(): UserDetailsDTO[] {
    const filtered = this.applyFilters(this.store.value.users);
    const start = (this.currentPage() - 1) * this.pageSize;
    return filtered.slice(start, start + this.pageSize);
  }

  // ── Pagination helpers ────────────────────────────────────────────────────

  get totalFiltered(): number {
    return this.applyFilters(this.store.value.users).length;
  }

  /**
   * Total pages we can display based on data already in the store.
   * If more data may exist on the server, we add +1 to keep Next enabled.
   */
  get totalPages(): number {
    const fromStore = Math.max(1, Math.ceil(this.totalFiltered / this.pageSize));
    return this.hasMoreData() ? fromStore + 1 : fromStore;
  }

  getPageNumbers(): number[] {
    const total = Math.max(1, Math.ceil(this.totalFiltered / this.pageSize));
    const current = this.currentPage();
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    const range: number[] = [];
    for (let i = start; i <= end; i++) range.push(i);
    return range;
  }

  /**
   * Navigate to a page.
   * - If we already have data for this page in the store → slice (no API call).
   * - If we are moving forward and data is missing → fetch from API.
   */
  goToPage(page: number): void {
    if (page < 1 || this.isLoading()) return;

    const alreadyFetched = this.store.pageCache.users.has(page);
    const dataExistsForPage =
      this.store.value.users.length >= page * this.pageSize || alreadyFetched;

    if (dataExistsForPage) {
      // Use cached data — no API call needed
      this.currentPage.set(page);
      return;
    }

    // Need to fetch from API
    this.fetchPage(page);
  }

  prevPage(): void {
    if (this.currentPage() <= 1 || this.isLoading()) return;
    // Prev always uses already-fetched data
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

    this.apiService.getAllUser(this.pagination).subscribe({
      next: (response: ApiResponse<GetUsersResponseDTO>) => {
        const incoming = response.data?.usersList ?? [];

        if (incoming.length === 0) {
          // No more records — server exhausted
          this.hasMoreData.set(false);
          toast.info('No more users to load');
        } else {
          this.store.appendUsers(incoming);
          this.store.pageCache.users.add(page);
          this.currentPage.set(page);

          // If the server returned fewer than a full page, we've reached the end
          if (incoming.length < this.pageSize) {
            this.hasMoreData.set(false);
          }
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        toast.error(err?.error?.message || 'Failed to load users');
        this.isLoading.set(false);
      },
    });
  }

  private refreshFilter(): void {
    this.currentPage.set(1);
  }

  updateSearch(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
    this.refreshFilter();
  }

  updateRoleFilter(event: Event): void {
    this.filterRole.set((event.target as HTMLSelectElement).value);
    this.refreshFilter();
  }

  // ── Actions ───────────────────────────────────────────────────────────────

  openDeleteModal(userId: string): void {
    this.pendingDeleteId.set(userId);
    this.confirmModal.set(true);
  }

  cancelDelete(): void {
    this.confirmModal.set(false);
    this.pendingDeleteId.set(null);
  }

  deleteUser(): void {
    const userId = this.pendingDeleteId();
    if (!userId) return;
    this.store.setUsers(this.store.value.users.filter(u => u.userId !== userId));
    this.confirmModal.set(false);
    this.pendingDeleteId.set(null);
    toast.success('User deleted successfully');
  }

  changeUserRole(userId: string, newRole: string): void {
    this.store.setUsers(
      this.store.value.users.map(u => u.userId === userId ? { ...u, role: newRole } : u)
    );
    toast.success(`User role updated to ${newRole}`);
  }

  getRoleColor(role: string): string {
    return role === 'admin' ? 'bg-purple-100 text-purple-800' : 'bg-blue-100 text-blue-800';
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'active':    return 'bg-green-100 text-green-800';
      case 'inactive':  return 'bg-gray-100 text-gray-800';
      case 'suspended': return 'bg-red-100 text-red-800';
      default:          return 'bg-gray-100 text-gray-800';
    }
  }
}