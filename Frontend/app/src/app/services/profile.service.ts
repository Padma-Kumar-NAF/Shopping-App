import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  EditUserDetailsModel,
  newEmailRequestDTO,
  GetUserByIdResponseDTO,
  EditMailResponseDTOModel,
} from '../models/users/profile.model';
import { ApiResponse } from '../models/users/apiResponse.model';

@Injectable({
  providedIn: 'root',
})
export class ProfileApiService {
  private baseUrl = 'https://localhost:7023/User';

  constructor(private Http: HttpClient) {}

  GetUserProfile(): Observable<ApiResponse<GetUserByIdResponseDTO>> {
    return this.Http.get<ApiResponse<GetUserByIdResponseDTO>>(`${this.baseUrl}/get-user-by-id`);
  }
  updateUserDetails(updatedUserDetails: EditUserDetailsModel): Observable<ApiResponse<EditMailResponseDTOModel>> {
    return this.Http.post<ApiResponse<EditMailResponseDTOModel>>(`${this.baseUrl}/update-user-details`, updatedUserDetails);
  }

  updateUserEmail(newEmailRequestDTO: newEmailRequestDTO): Observable<ApiResponse<EditMailResponseDTOModel>> {

    return this.Http.post<ApiResponse<EditMailResponseDTOModel>>(
      `${this.baseUrl}/edit-user-email`,
      newEmailRequestDTO,
    );
  }
}