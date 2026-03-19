import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { UserDetails } from '../models/user.model';
import { AuthApiService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  // Inject dependencies via inject() — no constructor parameter conflict
  private readonly platformId = inject(PLATFORM_ID);
  private readonly api = inject(AuthApiService);

  // ── Public signals ────────────────────────────────────────────────────────
  readonly user = signal<UserDetails | null>(null);
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly username = computed(() => this.user()?.userName ?? '');
  readonly role = computed(() => this.user()?.userRole ?? '');
  readonly email = computed(() => this.user()?.userEmail ?? '');

  // ── Auth actions ──────────────────────────────────────────────────────────

  /**
   * Called by provideAppInitializer on bootstrap.
   * Synchronously restores user state from localStorage.
   * Safe to call multiple times — idempotent.
   */
  loadUserFromStorage(): void {
    if (!isPlatformBrowser(this.platformId)) return; // No-op on server

    const token = localStorage.getItem('JWT-Token');
    if (!token) return;

    try {
      const decoded = this.api.decodeToken(token);
      if (!decoded) {
        this.clearUser();
        return;
      }
      // Set signal directly — do NOT call setUser() here to avoid
      // redundantly writing the token back to localStorage
      this.user.set(decoded);
    } catch {
      // Token is malformed or expired — clean up
      this.clearUser();
    }
  }

  /**
   * Called after a successful login API response.
   */
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