import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthStateService } from '../services/auth-state.service';

export const authInterceptors: HttpInterceptorFn = (req, next) => {
  const authState = inject(AuthStateService);

  const token =
    typeof window !== 'undefined' ? localStorage.getItem('JWT-Token') : null;

  const authReq = token
    ? req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authState.logout();
      }
      return throwError(() => error);
    }),
  );
};
