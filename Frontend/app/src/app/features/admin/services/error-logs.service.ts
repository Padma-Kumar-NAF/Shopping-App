import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/users/apiResponse.model';
import { PaginationModel } from '../../models/users/pagination.model';

export interface ErrorLogDTO {
  message: string;
  innerException: string;
  userName: string;
  role: string;
  controller: string;
  statusCode: number;
  createdAt: string;
}

export interface GetErrorLogsResponseDTO {
  items: ErrorLogDTO[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class ErrorLogsService {
  private baseUrl = 'https://localhost:7023/logs';

  constructor(private http: HttpClient) {}

  getErrorLogs(pagination: PaginationModel): Observable<ApiResponse<GetErrorLogsResponseDTO>> {
    return this.http.post<ApiResponse<GetErrorLogsResponseDTO>>(
      `${this.baseUrl}/get-logs`,
      { Pagination: pagination },
    );
  }
}
