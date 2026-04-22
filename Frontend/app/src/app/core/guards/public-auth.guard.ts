import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from '../state/auth-state.service';
import { RedirectService } from '../services/redirect.service';

export const authRequiredGuard: CanActivateFn = (route, state) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const redirectService = inject(RedirectService);

  const user = authState.user();

  if (!user) {
    redirectService.storeIntendedRoute(state.url);
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