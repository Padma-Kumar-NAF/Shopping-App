import { HttpInterceptorFn } from '@angular/common/http';
import { LoaderService } from '../services/loading.service';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';

export const authInterceptors: HttpInterceptorFn = (req, next) => {
  // const loader = inject(LoaderService);

  // loader.show();
  const token =
    // typeof window !== 'undefined' ? sessionStorage.getItem('JWT-Token') : null;
    typeof window !== 'undefined' ? localStorage.getItem('JWT-Token') : null;

  if (token) {
    // console.log("token")
    // console.log(token)
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`),
    });
    return next(authReq)
    // .pipe(
    //   finalize(() => {
    //     loader.hide();
    //   }),
    // );
  }

  return next(req)
  // .pipe(
  //   finalize(() => {
  //     loader.hide();
  //   }),
  // );
};
