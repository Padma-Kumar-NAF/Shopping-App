import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthStateService } from './auth-state.service';

export const authRequiredGuard: CanActivateFn = (route, state) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  const user = authState.user();

  if (!user) {
    if (typeof window !== 'undefined') {
      sessionStorage.setItem('redirectUrl', state.url);
      const queryParams = route.queryParams;
      if (Object.keys(queryParams).length > 0) {
        sessionStorage.setItem('redirectQueryParams', JSON.stringify(queryParams));
      }
    }

    return router.createUrlTree(['/auth']);
  }

  const expectedRole: string | undefined = route.data?.['role'];
  const userRole = user.userRole?.toLowerCase();

  if (expectedRole && userRole !== expectedRole.toLowerCase()) {
    const fallback = userRole === 'admin' ? '/admin' : '/';
    return router.createUrlTree([fallback]);
  }

  return true;
};

export const publicGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  const user = authState.user();
  if (user?.userRole?.toLowerCase() === 'admin') {
    return router.createUrlTree(['/admin/dashboard']);
  }

  return true;
};