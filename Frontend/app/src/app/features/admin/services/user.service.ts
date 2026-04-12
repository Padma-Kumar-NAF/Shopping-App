import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationModel } from '../../../shared/models/admin/pagination.model';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/admin/apiResponse.model';
import {ChangeUserRoleResponseDTO,DeactivateUserResponseDTO,GetUsersResponseDTO,} from '../../../shared/models/admin/users.model';

@Injectable({
  providedIn: 'root',
})
export class UserServcie {
  private readonly baseUrl = 'https://localhost:7023/User';

  constructor(private http: HttpClient) {

  }

  getAllUser(pagination: PaginationModel): Observable<ApiResponse<GetUsersResponseDTO>> {
    return this.http.post<ApiResponse<GetUsersResponseDTO>>(`${this.baseUrl}/get-all-users`, {pagination,});
  }

  deactivateUser(userId: string): Observable<ApiResponse<DeactivateUserResponseDTO>> {
    return this.http.post<ApiResponse<DeactivateUserResponseDTO>>(`${this.baseUrl}/delete-user`,{ UserId: userId },);
  }

  activateUser(userId: string): Observable<ApiResponse<DeactivateUserResponseDTO>> {
    return this.http.post<ApiResponse<DeactivateUserResponseDTO>>(`${this.baseUrl}/delete-user`,{ UserId: userId },);
  }

  changeUserRole(userId: string, role: string): Observable<ApiResponse<ChangeUserRoleResponseDTO>> {
    return this.http.post<ApiResponse<ChangeUserRoleResponseDTO>>(`${this.baseUrl}/change-user-role`,{ UserId: userId, Role: role },);
  }
}
