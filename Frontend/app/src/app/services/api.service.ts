import { HttpClient } from '@angular/common/http';
import {
  CreateUserResponseDTO,
  LoginModel,
  SignupModel,
  LoginResponseDTO,
} from '../models/login.model';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthApiService {

  private baseUrl = 'https://localhost:7023/';

  constructor(private http: HttpClient) {}

  LoginApi(loginModel: LoginModel): Observable<LoginResponseDTO> {
    return this.http.post<LoginResponseDTO>(
      `${this.baseUrl}api/auth/login`,
      loginModel
    );
  }

  SignUpApi(signUpModel: SignupModel): Observable<CreateUserResponseDTO> {
    return this.http.post<CreateUserResponseDTO>(
      `${this.baseUrl}api/auth/register`,
      signUpModel
    );
  }
}