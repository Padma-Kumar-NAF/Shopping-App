import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { UserDetails } from '../models/user.model';
import { AuthApiService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly api = inject(AuthApiService);

  readonly user = signal<UserDetails | null>(null);
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly username = computed(() => this.user()?.userName ?? '');
  readonly role = computed(() => this.user()?.userRole ?? '');
  readonly email = computed(() => this.user()?.userEmail ?? '');

  loadUserFromStorage(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const token = localStorage.getItem('JWT-Token');
    if (!token) return;

    try {
      const decoded = this.api.decodeToken(token);
      if (!decoded) {
        this.clearUser();
        return;
      }
      this.user.set(decoded);
    } catch {
      this.clearUser();
    }
  }

  setUser(userInfo: UserDetails | null, token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('JWT-Token', token);
    }
    this.user.set(userInfo);
  }

  clearUser(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('JWT-Token');
    }
    this.user.set(null);
  }
}