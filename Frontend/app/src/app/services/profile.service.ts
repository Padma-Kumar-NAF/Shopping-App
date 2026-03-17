import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  EditMailRequestDTOModel,
  EditUserDetailsModel,
  newEmailRequestDTO,
  UserProfile,
} from '../models/profile.model';

@Injectable({
  providedIn: 'root',
})
export class ProfileApiService {
  private baseUrl = 'https://localhost:7023/User';

  constructor(private Http: HttpClient) {}

  GetUserProfile(): Observable<UserProfile> {
    return this.Http.get<UserProfile>(`${this.baseUrl}/GetUserById`);
  }

  updateUserDetails(updatedUserDetails: EditUserDetailsModel) {
    return this.Http.post(`${this.baseUrl}/UpdateUserDetails`, updatedUserDetails);
  }

  updateUserEmail(newEmailRequestDTO: newEmailRequestDTO): Observable<EditMailRequestDTOModel> {

    return this.Http.post<EditMailRequestDTOModel>(
      `${this.baseUrl}/EditUserMail`,
      newEmailRequestDTO,
    );
  }
}