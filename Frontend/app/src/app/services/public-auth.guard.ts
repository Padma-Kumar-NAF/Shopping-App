import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthStateService } from './auth-state.service';

/**
 * Guard for routes that require authentication.
 * Stores the intended destination and redirects to login if not authenticated.
 */
export const authRequiredGuard: CanActivateFn = (route, state) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  const user = authState.user();

  if (!user) {
    // Store the intended URL for post-login redirect
    if (typeof window !== 'undefined') {
      sessionStorage.setItem('redirectUrl', state.url);

      // Store query params if present (for product details, etc.)
      const queryParams = route.queryParams;
      if (Object.keys(queryParams).length > 0) {
        sessionStorage.setItem('redirectQueryParams', JSON.stringify(queryParams));
      }
    }

    // Redirect to auth page
    return router.createUrlTree(['/auth']);
  }

  // Check role if specified
  const expectedRole: string | undefined = route.data?.['role'];
  const userRole = user.userRole?.toLowerCase();

  if (expectedRole && userRole !== expectedRole.toLowerCase()) {
    const fallback = userRole === 'admin' ? '/admin' : '/';
    return router.createUrlTree([fallback]);
  }

  return true;
};

/**
 * Guard for public routes that should be accessible without authentication.
 * No redirect, just allows access.
 */
export const publicGuard: CanActivateFn = () => {
  return true;
};
