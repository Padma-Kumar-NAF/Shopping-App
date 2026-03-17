import { HttpClient } from '@angular/common/http';
import {
  CreateUserResponseDTO,
  LoginModel,
  SignupModel,
  LoginResponseDTO,
} from '../models/auth.model';
import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class AuthApiService {

  private baseUrl = 'https://localhost:7023/';
  private platformId = inject(PLATFORM_ID);

  constructor(private http: HttpClient) {}

  LoginApi(loginModel: LoginModel): Observable<LoginResponseDTO> {
    return this.http.post<LoginResponseDTO>(
      `${this.baseUrl}api/auth/login`,
      loginModel
    );
  }

  SignUpApi(user: SignupModel): Observable<CreateUserResponseDTO> {
    return this.http.post<CreateUserResponseDTO>(
      `${this.baseUrl}api/auth/register`,
      user
    ).pipe(
      catchError((error) => {
        return throwError(() => error);
      })
    );
  }

  isAuthenticated(): boolean {

    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem("JWT-Token") != null;
    }

    return false;
  }
}