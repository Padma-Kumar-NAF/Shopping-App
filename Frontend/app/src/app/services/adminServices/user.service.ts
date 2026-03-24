import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationModel } from '../../models/admin/pagination.model';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/admin/apiResponse.model';
import {
  ChangeUserRoleResponseDTO,
  DeactivateUserResponseDTO,
  GetUsersResponseDTO,
} from '../../models/admin/users.model';

@Injectable({
  providedIn: 'root',
})
export class UserServcie {
  private readonly baseUrl = 'https://localhost:7023/User';

  constructor(private http: HttpClient) {}

  getAllUser(pagination: PaginationModel): Observable<ApiResponse<GetUsersResponseDTO>> {
    return this.http.post<ApiResponse<GetUsersResponseDTO>>(`${this.baseUrl}/get-all-users`, {
      pagination,
    });
  }

  deactivateUser(userId: string): Observable<ApiResponse<DeactivateUserResponseDTO>> {
    return this.http.post<ApiResponse<DeactivateUserResponseDTO>>(
      `${this.baseUrl}/deactivate-user`,
      { UserId: userId },
    );
  }

  activateUser(userId: string): Observable<ApiResponse<DeactivateUserResponseDTO>> {
    return this.http.post<ApiResponse<DeactivateUserResponseDTO>>(
      `${this.baseUrl}/activate-user`,
      { UserId: userId },
    );
  }

  changeUserRole(userId: string, role: string): Observable<ApiResponse<ChangeUserRoleResponseDTO>> {
    return this.http.post<ApiResponse<ChangeUserRoleResponseDTO>>(
      `${this.baseUrl}/change-user-role`,
      { UserId: userId, Role: role },
    );
  }
}
