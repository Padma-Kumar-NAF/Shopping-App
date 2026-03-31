import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from './auth-state.service';

export const roleGuard: CanActivateFn = (route) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  const user = authState.user();

  if (!user) {
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