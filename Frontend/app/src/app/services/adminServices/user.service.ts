import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationModel } from '../../models/admin/pagination.model';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/admin/apiResponse.model';
import { GetUsersResponseDTO } from '../../models/admin/users.model';

@Injectable({
  providedIn: 'root',
})
export class UserServcie {
  private readonly baseUrl = 'https://localhost:7023/User';
  constructor(private http: HttpClient) {}

  getAllUser(pagination: PaginationModel): Observable<ApiResponse<GetUsersResponseDTO>> {
    const body = {
      pagination: pagination,
    };
    return this.http.post<ApiResponse<GetUsersResponseDTO>>(`${this.baseUrl}/get-all-users`, body);
  }
}