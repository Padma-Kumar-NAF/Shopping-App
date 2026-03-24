import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/users/apiResponse.model';
import {
  AddPromoCodeRequestDTO,
  AddPromoCodeResponseDTO,
  ValidatePromoCodeRequestDTO,
  ValidatePromoCodeResponseDTO,
} from '../../models/admin/promocode.model';

@Injectable({ providedIn: 'root' })
export class PromoCodeService {
  private baseUrl = 'https://localhost:7023/promocode';

  constructor(private http: HttpClient) {}

  addPromoCode(request: AddPromoCodeRequestDTO): Observable<ApiResponse<AddPromoCodeResponseDTO>> {
    return this.http.post<ApiResponse<AddPromoCodeResponseDTO>>(
      `${this.baseUrl}/add-promocode`,
      request,
    );
  }

  validatePromoCode(request: ValidatePromoCodeRequestDTO): Observable<ApiResponse<ValidatePromoCodeResponseDTO>> {
    return this.http.post<ApiResponse<ValidatePromoCodeResponseDTO>>(
      `${this.baseUrl}/verify-promocode`,
      request,
    );
  }
}
