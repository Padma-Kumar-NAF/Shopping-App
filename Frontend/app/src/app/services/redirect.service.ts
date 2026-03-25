import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class RedirectService {
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

  storeIntendedRoute(url: string, queryParams?: any): void {
    if (!isPlatformBrowser(this.platformId)) return;

    // Never store the auth page itself as a redirect target
    if (url.startsWith('/auth')) return;

    sessionStorage.setItem('redirectUrl', url);
    if (queryParams && Object.keys(queryParams).length > 0) {
      sessionStorage.setItem('redirectQueryParams', JSON.stringify(queryParams));
    }
  }

  getRedirectUrl(): string | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    return sessionStorage.getItem('redirectUrl');
  }

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

  navigateToIntendedRoute(defaultRoute: string = '/'): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const redirectUrl = this.getRedirectUrl();
    this.clearRedirectData();

    if (redirectUrl) {
      // navigateByUrl handles the full URL including query params and fragments
      this.router.navigateByUrl(redirectUrl);
    } else {
      this.router.navigate([defaultRoute]);
    }
  }

  clearRedirectData(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    sessionStorage.removeItem('redirectUrl');
    sessionStorage.removeItem('redirectQueryParams'); // kept for backward compat cleanup
  }

  hasPendingRedirect(): boolean {
    if (!isPlatformBrowser(this.platformId)) return false;
    return sessionStorage.getItem('redirectUrl') !== null;
  }
}
