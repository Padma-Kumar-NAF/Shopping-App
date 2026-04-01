import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { UserDetails } from '../../shared/models/users/user.model';
import { AuthApiService } from '../services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly api = inject(AuthApiService);
  private readonly router = inject(Router);

  private logoutTimer: ReturnType<typeof setTimeout> | null = null;

  readonly user = signal<UserDetails | null>(null);
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly username = computed(() => this.user()?.userName ?? '');
  readonly role = computed(() => this.user()?.userRole ?? '');
  readonly email = computed(() => this.user()?.userEmail ?? '');

  getTokenExpiryMs(token: string): number | null {
    try {
      const decoded: any = jwtDecode(token);
      if (!decoded?.exp) return null;
      const msLeft = decoded.exp * 1000 - Date.now();
      // console.log("msLeft");
      // console.log(msLeft);
      return msLeft > 0 ? msLeft : null;
    } catch {
      return null;
    }
  }

  scheduleAutoLogout(token: string): void {
    this.cancelAutoLogout();
    const msLeft = this.getTokenExpiryMs(token);
    if (msLeft === null) {
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

  clearUser(): void {
    this.cancelAutoLogout();
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('JWT-Token');
    }
    this.user.set(null);
  }

  logout(): void {
    this.clearUser();
    this.router.navigate(['/auth']);
  }
}