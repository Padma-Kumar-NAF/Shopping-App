import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { User } from '../../../models/users/admin.model';
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

  // Confirm delete modal state
  confirmModal = signal<boolean>(false);
  pendingDeleteId = signal<string | null>(null);

  // Filter signals
  searchTerm = signal<string>('');
  filterRole = signal<string>('all');
  filterStatus = signal<string>('all');

  pagination: PaginationModel;

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = 10;
  }

  ngOnInit(): void {
    this.users$ = this.store.state$.pipe(map(s => s.users));

    this.filteredUsers$ = this.store.state$.pipe(
      map(s => {
        let filtered = s.users;

        const term = this.searchTerm().toLowerCase();
        if (term) {
          filtered = filtered.filter(
            u => u.name.toLowerCase().includes(term) || u.email.toLowerCase().includes(term)
          );
        }

        if (this.filterRole() !== 'all') {
          filtered = filtered.filter(u => u.role === this.filterRole());
        }

        if (this.filterStatus() !== 'all') {
          filtered = filtered.filter(u => (u as any).status === this.filterStatus());
        }

        return filtered;
      })
    );
  }

  // Trigger re-evaluation of filteredUsers$ by updating the store reference
  private refreshFilter(): void {
    const current = this.store.value.users;
    this.store.setUsers([...current]);
  }

  updateSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.refreshFilter();
  }

  updateRoleFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterRole.set(value);
    this.refreshFilter();
  }

  updateStatusFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterStatus.set(value);
    this.refreshFilter();
  }

  // Opens confirm modal and stores the id to delete
  openDeleteModal(userId: string): void {
    this.pendingDeleteId.set(userId);
    this.confirmModal.set(true);
  }

  cancelDelete(): void {
    this.confirmModal.set(false);
    this.pendingDeleteId.set(null);
  }

  // Called when user confirms deletion
  deleteUser(): void {
    const userId = this.pendingDeleteId();
    if (!userId) return;

    console.log('Deleting user with id:', userId);

    // Update store
    const updatedUsers = this.store.value.users.filter(u => u.userId !== userId);
    this.store.setUsers(updatedUsers);

    this.confirmModal.set(false);
    this.pendingDeleteId.set(null);

    toast.success('User deleted successfully');
  }

  changeUserRole(userId: string, newRole: string): void {
    const updatedUsers = this.store.value.users.map(u =>
      u.userId === userId ? { ...u, role: newRole } : u
    );
    this.store.setUsers(updatedUsers);
    toast.success(`User role updated to ${newRole}`);
  }

  changeUserStatus(userId: string, newStatus: string): void {
    const updatedUsers = this.store.value.users.map(u =>
      u.userId === userId ? { ...u, status: newStatus } : u
    );
    this.store.setUsers(updatedUsers);
    toast.success(`User status updated to ${newStatus}`);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'active': return 'bg-green-100 text-green-800';
      case 'inactive': return 'bg-gray-100 text-gray-800';
      case 'suspended': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getRoleColor(role: string): string {
    return role === 'admin' ? 'bg-purple-100 text-purple-800' : 'bg-blue-100 text-blue-800';
  }
}