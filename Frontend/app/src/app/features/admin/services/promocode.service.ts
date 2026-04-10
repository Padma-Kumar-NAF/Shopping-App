import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/users/apiResponse.model';
import {
  AddPromoCodeRequestDTO,
  AddPromoCodeResponseDTO,
  EditPromoCodeRequestDTO,
  EditPromoCodeResponseDTO,
  DeletePromoCodeRequestDTO,
  DeletePromoCodeResponseDTO,
  GetAllPromocodesRequestDTO,
  GetAllPromocodesResponseDTO,
  ValidatePromoCodeRequestDTO,
  ValidatePromoCodeResponseDTO,
} from '../../../shared/models/admin/promocode.model';
import { GetAllUserPromoCodesResponseDTO } from '../../../shared/models/users/promoCode.model';

@Injectable({ providedIn: 'root' })
export class PromoCodeService {
  private baseUrl = 'https://localhost:7023/PromoCode';

  constructor(private http: HttpClient) {}

  getAllPromoCodes(pageNumber: number, pageSize: number): Observable<ApiResponse<GetAllPromocodesResponseDTO>> {
    const body: GetAllPromocodesRequestDTO = { pagination: { pageNumber, pageSize } };
    return this.http.post<ApiResponse<GetAllPromocodesResponseDTO>>(`${this.baseUrl}/get-all-promocode`, body);
  }

  addPromoCode(request: AddPromoCodeRequestDTO): Observable<ApiResponse<AddPromoCodeResponseDTO>> {
    return this.http.post<ApiResponse<AddPromoCodeResponseDTO>>(`${this.baseUrl}/add-promocode`, request);
  }

  editPromoCode(request: EditPromoCodeRequestDTO): Observable<ApiResponse<EditPromoCodeResponseDTO>> {
    return this.http.post<ApiResponse<EditPromoCodeResponseDTO>>(`${this.baseUrl}/edit-promocode`, request);
  }

  deletePromoCode(request: DeletePromoCodeRequestDTO): Observable<ApiResponse<DeletePromoCodeResponseDTO>> {
    return this.http.delete<ApiResponse<DeletePromoCodeResponseDTO>>(`${this.baseUrl}/delete-promocode`, { body: request });
  }

  validatePromoCode(request: ValidatePromoCodeRequestDTO): Observable<ApiResponse<ValidatePromoCodeResponseDTO>> {
    return this.http.post<ApiResponse<ValidatePromoCodeResponseDTO>>(`https://localhost:7023/UserPromoCode/verify-promocode`, request);
  }

  getAllUserPromo() : Observable<ApiResponse<GetAllUserPromoCodesResponseDTO>>{
    return this.http.get<ApiResponse<GetAllUserPromoCodesResponseDTO>>(`https://localhost:7023/UserPromoCode/get-user-promocodes`);
  }
}