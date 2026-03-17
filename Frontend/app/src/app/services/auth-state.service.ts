import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
    providedIn: 'root',
})
export class AuthStateService {
    private platformId = inject(PLATFORM_ID);

    // Reactive signal for the username stored in localStorage
    private _username = signal<string>('');

    // Reactive signal for authentication state
    private _isAuthenticated = signal<boolean>(false);

    // Public computed read-only signals
    readonly username = computed(() => this._username());
    readonly isAuthenticated = computed(() => this._isAuthenticated());

    /** Call on service initialisation and after login/logout */
    syncFromStorage(): void {
        if (isPlatformBrowser(this.platformId)) {
            const token = localStorage.getItem('JWT-Token');
            const name = localStorage.getItem('User-Name') ?? '';
            this._isAuthenticated.set(token !== null);
            this._username.set(name);
        }
    }

    /** Called after a successful login */
    setUser(name: string, token: string): void {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('JWT-Token', token);
            localStorage.setItem('User-Name', name);
        }
        this._username.set(name);
        this._isAuthenticated.set(true);
    }

    /** Called on logout */
    clearUser(): void {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.removeItem('JWT-Token');
            localStorage.removeItem('User-Name');
        }
        this._username.set('');
        this._isAuthenticated.set(false);
    }
}
