import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { User } from '../../../models/admin.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-users-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './users-management.html',
  styleUrl: './users-management.css',
})
export class UsersManagement {
  users = signal<User[]>([
    {
      id: '1',
      name: 'John Doe',
      email: 'john@example.com',
      role: 'customer',
      status: 'active',
      createdAt: new Date('2024-01-15'),
    },
    {
      id: '2',
      name: 'Jane Smith',
      email: 'jane@example.com',
      role: 'customer',
      status: 'active',
      createdAt: new Date('2024-02-20'),
    },
    {
      id: '3',
      name: 'Admin User',
      email: 'admin@example.com',
      role: 'admin',
      status: 'active',
      createdAt: new Date('2023-12-01'),
    },
    {
      id: '4',
      name: 'Bob Johnson',
      email: 'bob@example.com',
      role: 'customer',
      status: 'inactive',
      createdAt: new Date('2024-03-10'),
    },
    {
      id: '5',
      name: 'Alice Williams',
      email: 'alice@example.com',
      role: 'customer',
      status: 'suspended',
      createdAt: new Date('2024-01-25'),
    },
  ]);

  searchTerm = signal<string>('');
  filterRole = signal<string>('all');
  filterStatus = signal<string>('all');

  get filteredUsers(): User[] {
    let filtered = this.users();

    // Search filter
    if (this.searchTerm()) {
      const term = this.searchTerm().toLowerCase();
      filtered = filtered.filter(
        (user) => user.name.toLowerCase().includes(term) || user.email.toLowerCase().includes(term)
      );
    }

    // Role filter
    if (this.filterRole() !== 'all') {
      filtered = filtered.filter((user) => user.role === this.filterRole());
    }

    // Status filter
    if (this.filterStatus() !== 'all') {
      filtered = filtered.filter((user) => user.status === this.filterStatus());
    }

    return filtered;
  }

  updateSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
  }

  updateRoleFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterRole.set(value);
  }

  updateStatusFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterStatus.set(value);
  }

  changeUserStatus(userId: string, newStatus: User['status']): void {
    const updatedUsers = this.users().map((user) =>
      user.id === userId ? { ...user, status: newStatus } : user
    );
    this.users.set(updatedUsers);
    toast.success(`User status updated to ${newStatus}`);
  }

  changeUserRole(userId: string, newRole: User['role']): void {
    const updatedUsers = this.users().map((user) =>
      user.id === userId ? { ...user, role: newRole } : user
    );
    this.users.set(updatedUsers);
    toast.success(`User role updated to ${newRole}`);
  }

  deleteUser(userId: string): void {
    const updatedUsers = this.users().filter((user) => user.id !== userId);
    this.users.set(updatedUsers);
    toast.success('User deleted successfully');
  }

  getStatusColor(status: User['status']): string {
    switch (status) {
      case 'active':
        return 'bg-green-100 text-green-800';
      case 'inactive':
        return 'bg-gray-100 text-gray-800';
      case 'suspended':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  getRoleColor(role: User['role']): string {
    return role === 'admin' ? 'bg-purple-100 text-purple-800' : 'bg-blue-100 text-blue-800';
  }
}
