import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { UserDetails } from '../models/users/user.model';
import { AuthApiService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly api = inject(AuthApiService);
  private readonly router = inject(Router);

  private logoutTimer: ReturnType<typeof setTimeout> | null = null;

  readonly user = signal<UserDetails | null>(null);
  readonly isAuthenticated = computed(() => this.user() !== null); // If there is any changes if user then this will run
  readonly username = computed(() => this.user()?.userName ?? '');
  readonly role = computed(() => this.user()?.userRole ?? '');
  readonly email = computed(() => this.user()?.userEmail ?? ''); // these all will run if user then this will run

  /** Returns ms until token expires, or null if token is missing/invalid/already expired. */
  getTokenExpiryMs(token: string): number | null {
    try {
      const decoded: any = jwtDecode(token);
      if (!decoded?.exp) return null;
      const msLeft = decoded.exp * 1000 - Date.now();
      return msLeft > 0 ? msLeft : null;
    } catch {
      return null;
    }
  }

  /** Schedules an automatic logout when the token expires. */
  scheduleAutoLogout(token: string): void {
    this.cancelAutoLogout();
    const msLeft = this.getTokenExpiryMs(token);
    if (msLeft === null) {
      // Token already expired — log out immediately
      this.logout();
      return;
    }
    this.logoutTimer = setTimeout(() => this.logout(), msLeft);
  }

  cancelAutoLogout(): void {
    if (this.logoutTimer !== null) {
      clearTimeout(this.logoutTimer);
      this.logoutTimer = null;
    }
  }

  loadUserFromStorage(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const token = localStorage.getItem('JWT-Token');
    if (!token) return;

    // Reject expired tokens immediately
    if (this.getTokenExpiryMs(token) === null) {
      this.clearUser();
      return;
    }

    try {
      const decoded = this.api.decodeToken(token);
      if (!decoded) {
        this.clearUser();
        return;
      }
      this.user.set(decoded);
      this.scheduleAutoLogout(token);
    } catch {
      this.clearUser();
    }
  }

  setUser(userInfo: UserDetails | null, token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('JWT-Token', token);
    }
    this.user.set(userInfo);
    this.scheduleAutoLogout(token);
  }

  /** Clears auth state and storage without redirecting. */
  clearUser(): void {
    this.cancelAutoLogout();
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('JWT-Token');
    }
    this.user.set(null);
  }

  /** Clears auth state and redirects to /auth. */
  logout(): void {
    this.clearUser();
    this.router.navigate(['/auth']);
  }
}