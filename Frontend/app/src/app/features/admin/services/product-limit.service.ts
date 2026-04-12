import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/admin/apiResponse.model';
import { PaginationModel } from '../../../shared/models/admin/pagination.model';
import {
  AddLimitRequestDTO,
  AddLimitResponseDTO,
  DeleteLimitRequestDTO,
  DeleteLimitResponseDTO,
  EditLimitRequestDTO,
  EditLimitResponseDTO,
  GetAllLimitsResponseDTO,
} from '../../../shared/models/admin/product-limit.model';

@Injectable({
  providedIn: 'root',
})
export class ProductLimitService {
  private baseUrl = 'https://localhost:7023/UserMonthlyProductLimit';

  constructor(private http: HttpClient) {
    
  }

  getAllLimits(pagination: PaginationModel): Observable<ApiResponse<GetAllLimitsResponseDTO>> {
    return this.http.post<ApiResponse<GetAllLimitsResponseDTO>>(`${this.baseUrl}/get-limits`, {Pagination: pagination,});
  }

  addLimit(payload: AddLimitRequestDTO): Observable<ApiResponse<AddLimitResponseDTO>> {
    return this.http.put<ApiResponse<AddLimitResponseDTO>>(`${this.baseUrl}/add-limit`, payload);
  }

  editLimit(payload: EditLimitRequestDTO): Observable<ApiResponse<EditLimitResponseDTO>> {
    return this.http.post<ApiResponse<EditLimitResponseDTO>>(`${this.baseUrl}/edit-limit`,payload,);
  }

  deleteLimit(payload: DeleteLimitRequestDTO): Observable<ApiResponse<DeleteLimitResponseDTO>> {
    return this.http.delete<ApiResponse<DeleteLimitResponseDTO>>(`${this.baseUrl}/delete-limit`,{ body: payload },);
  }
}
