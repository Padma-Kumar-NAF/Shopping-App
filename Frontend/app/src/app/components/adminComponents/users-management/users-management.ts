import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toast } from 'ngx-sonner';
import { UserServcie } from '../../../services/adminServices/user.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { ApiResponse } from '../../../models/admin/apiResponse.model';
import { GetUsersResponseDTO, UserDetailsDTO } from '../../../models/admin/users.model';
import { StoreService } from '../../../services/adminServices/store.service';
import { AuthStateService } from '../../../services/auth-state.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-users-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users-management.html',
  styleUrl: './users-management.css',
})
export class UsersManagement implements OnInit {
  private readonly apiService = inject(UserServcie);
  store = inject(StoreService);
  private readonly authState = inject(AuthStateService);

  users$!: Observable<UserDetailsDTO[]>;
  filteredUsers$!: Observable<UserDetailsDTO[]>;

  // Status toggle confirm modal
  confirmModal = signal<boolean>(false);
  pendingToggleId = signal<string | null>(null);
  pendingToggleAction = signal<'activate' | 'deactivate'>('deactivate');

  searchTerm = signal<string>('');
  filterRole = signal<string>('all');

  // Pagination
  currentPage = signal<number>(1);
  readonly pageSize = 10;
  hasMoreData = signal<boolean>(true);
  isLoading = signal<boolean>(false);

  // Per-row loading states
  roleChangingId = signal<string | null>(null);
  statusTogglingId = signal<string | null>(null);

  pagination: PaginationModel;

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = this.pageSize;
  }

  ngOnInit(): void {
    this.users$ = this.store.state$.pipe(map(s => s.users));
    this.filteredUsers$ = this.store.state$.pipe(map(s => this.applyFilters(s.users)));

    // Load first page if store is empty (e.g. direct navigation)
    if (this.store.value.users.length === 0) {
      this.fetchPage(1);
    }
  }

  // ── Filters ───────────────────────────────────────────────────────────────

  private applyFilters(users: UserDetailsDTO[]): UserDetailsDTO[] {
    const currentEmail = this.authState.email();
    let filtered = users.filter(u => u.email !== currentEmail);
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

  get pagedUsers(): UserDetailsDTO[] {
    console.log(this.store.value.users)
    const filtered = this.applyFilters(this.store.value.users);
    const start = (this.currentPage() - 1) * this.pageSize;
    return filtered.slice(start, start + this.pageSize);
  }

  get totalFiltered(): number {
    return this.applyFilters(this.store.value.users).length;
  }

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

  goToPage(page: number): void {
    if (page < 1 || this.isLoading()) return;
    const alreadyFetched = this.store.pageCache.users.has(page);
    const dataExistsForPage =
      this.store.value.users.length >= page * this.pageSize || alreadyFetched;
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

    this.apiService.getAllUser(this.pagination).subscribe({
      next: (response: ApiResponse<GetUsersResponseDTO>) => {
        const incoming = response.data?.usersList ?? [];
        if (incoming.length === 0) {
          this.hasMoreData.set(false);
          toast.info('No more users to load');
        } else {
          this.store.setUsers(incoming);
          this.store.pageCache.users.add(page);
          this.currentPage.set(page);
          if (incoming.length < this.pageSize) this.hasMoreData.set(false);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
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

  // ── Role change ───────────────────────────────────────────────────────────

  changeUserRole(userId: string, newRole: string): void {
    const previousRole = this.store.value.users.find(u => u.userId === userId)?.role;
    this.roleChangingId.set(userId);
    this.apiService.changeUserRole(userId, newRole).subscribe({
      next: (res) => {
        this.roleChangingId.set(null);
        const user = this.store.value.users.find(u => u.userId === userId);
        if (!user) return;
        if (res.data?.isChanged) {
          this.store.updateUser({ ...user, role: newRole });
          toast.success(`Role updated to "${newRole}"`);
        } else {
          toast.info(res.message || 'User already has this role');
        }
      },
      error: (err) => {
        this.roleChangingId.set(null);
        const user = this.store.value.users.find(u => u.userId === userId);
        if (user && previousRole) {
          this.store.updateUser({ ...user, role: previousRole });
          console.log(previousRole)
        }
        toast.error(err?.error?.message || 'Failed to update role');
      },
    });
  }

  // ── Status toggle ─────────────────────────────────────────────────────────

  openStatusModal(userId: string, currentlyActive: boolean): void {
    this.pendingToggleId.set(userId);
    this.pendingToggleAction.set(currentlyActive ? 'deactivate' : 'activate');
    this.confirmModal.set(true);
  }

  cancelStatusToggle(): void {
    this.confirmModal.set(false);
    this.pendingToggleId.set(null);
  }

  confirmStatusToggle(): void {
    const userId = this.pendingToggleId();
    const action = this.pendingToggleAction();
    if (!userId) return;
    this.confirmModal.set(false);
    this.pendingToggleId.set(null);
    this.statusTogglingId.set(userId);

    const call$ = action === 'deactivate'
      ? this.apiService.deactivateUser(userId)
      : this.apiService.activateUser(userId);

    call$.subscribe({
      next: (_res) => {
        this.statusTogglingId.set(null);
        const user = this.store.value.users.find(u => u.userId === userId);
        if (!user) return;
        const newActive = action === 'activate';
        this.store.updateUser({ ...user, activeStatus: newActive });
        toast.success(`User ${newActive ? 'activated' : 'deactivated'} successfully`);
      },
      error: (err) => {
        this.statusTogglingId.set(null);
        toast.error(err?.error?.message || `Failed to ${action} user`);
      },
    });
  }

  // ── UI helpers ────────────────────────────────────────────────────────────

  getRoleColor(role: string): string {
    return role === 'admin'
      ? 'bg-purple-100 text-purple-800'
      : 'bg-blue-100 text-blue-800';
  }
}
