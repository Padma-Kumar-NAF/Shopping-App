import { HttpInterceptorFn } from "@angular/common/http";

export const authInterceptors: HttpInterceptorFn = (req, next) => {
  const token =
    typeof window !== 'undefined' ? sessionStorage.getItem('JWT-Token') : null;

  if (token) {
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authReq);
  }

  return next(req);
};