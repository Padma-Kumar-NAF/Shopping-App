import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthApiService } from '../services/api.service';

export const authGuard = () => {

  const authService = inject(AuthApiService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/auth']);
  return false;
};