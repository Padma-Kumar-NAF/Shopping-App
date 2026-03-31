import { HttpClient } from '@angular/common/http';
import {
  CreateUserResponseDTO,
  LoginModel,
  SignupModel,
  LoginResponseDTO,
} from '../models/users/auth.model';
import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';
import { jwtDecode } from 'jwt-decode';
import { UserDetails } from '../models/users/user.model';
import { ApiResponse } from '../models/users/apiResponse.model';

@Injectable({
  providedIn: 'root',
})
export class AuthApiService {
  private baseUrl = 'https://localhost:7023/';
  private platformId = inject(PLATFORM_ID);

  constructor(private http: HttpClient) {}

  LoginApi(loginModel: LoginModel): Observable<ApiResponse<LoginResponseDTO>> {
    return this.http.post<ApiResponse<LoginResponseDTO>>(`${this.baseUrl}api/auth/login`, loginModel);
  }

  SignUpApi(user: SignupModel): Observable<CreateUserResponseDTO> {
    return this.http.post<CreateUserResponseDTO>(`${this.baseUrl}api/auth/register`, user).pipe(
      catchError((error) => {
        return throwError(() => error);
      }),
    );
  }

  decodeToken(token: string): UserDetails | null {
    try {
      const decoded: any = jwtDecode(token);
      const userInfo: UserDetails = new UserDetails();
      userInfo.userName =
        decoded.name || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];

      userInfo.userEmail =
        decoded.email ||
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];

      userInfo.userRole =
        decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      return userInfo;
    } catch (error) {
      console.error('Invalid token');
      return null;
    }
  }
}