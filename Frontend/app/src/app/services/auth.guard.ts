import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from './auth-state.service';

export const roleGuard: CanActivateFn = (route) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  // By the time any guard runs, provideAppInitializer has already completed,
  // so user() is guaranteed to be populated if a valid token exists.
  const user = authState.user();

  if (!user) {
    return router.createUrlTree(['/auth']);
  }

  const expectedRole: string | undefined = route.data?.['role'];
  const userRole = user.userRole?.toLowerCase();

  if (expectedRole && userRole !== expectedRole.toLowerCase()) {
    // Redirect to the correct home for this role instead of blocking
    const fallback = userRole === 'admin' ? '/admin' : '/';
    return router.createUrlTree([fallback]);
  }

  return true;
};