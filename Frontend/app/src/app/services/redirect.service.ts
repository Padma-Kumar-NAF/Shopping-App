import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class RedirectService {
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

  /**
   * Store the intended URL and query params before redirecting to login
   */
  storeIntendedRoute(url: string, queryParams?: any): void {
    if (!isPlatformBrowser(this.platformId)) return;

    sessionStorage.setItem('redirectUrl', url);
    if (queryParams && Object.keys(queryParams).length > 0) {
      sessionStorage.setItem('redirectQueryParams', JSON.stringify(queryParams));
    }
  }

  /**
   * Get the stored redirect URL
   */
  getRedirectUrl(): string | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    return sessionStorage.getItem('redirectUrl');
  }

  /**
   * Get the stored query params
   */
  getRedirectQueryParams(): any {
    if (!isPlatformBrowser(this.platformId)) return null;

    const params = sessionStorage.getItem('redirectQueryParams');
    if (params) {
      try {
        return JSON.parse(params);
      } catch {
        return null;
      }
    }
    return null;
  }

  /**
   * Navigate to the stored redirect URL or default route
   */
  navigateToIntendedRoute(defaultRoute: string = '/'): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const redirectUrl = this.getRedirectUrl();
    const queryParams = this.getRedirectQueryParams();

    // Clear stored values
    this.clearRedirectData();

    if (redirectUrl) {
      // Navigate to the stored URL with query params
      this.router.navigate([redirectUrl], { queryParams: queryParams || {} });
    } else {
      // Navigate to default route
      this.router.navigate([defaultRoute]);
    }
  }

  /**
   * Clear stored redirect data
   */
  clearRedirectData(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    sessionStorage.removeItem('redirectUrl');
    sessionStorage.removeItem('redirectQueryParams');
  }

  /**
   * Check if there's a pending redirect
   */
  hasPendingRedirect(): boolean {
    if (!isPlatformBrowser(this.platformId)) return false;
    return sessionStorage.getItem('redirectUrl') !== null;
  }
}
