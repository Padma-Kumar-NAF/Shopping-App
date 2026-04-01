import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationModel } from '../../../shared/models/users/pagination.model';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  AddCategoryRequestDTO,
  AddCategoryResponseDTO,
  DeleteCategoryRequestDTO,
  DeleteCategoryResponseDTO,
  EditCategoryRequestDTO,
  EditCategoryResponseDTO,
  GetAllCategoryResponseDTO,
} from '../../../shared/models/admin/categories.model';
import { ApiResponse } from '../../../shared/models/users/apiResponse.model';

@Injectable({
  providedIn: 'root',
})
export class AdminCategoryService {
  private baseUrl = 'https://localhost:7023/category';
  constructor(private http: HttpClient) {}

  getAllCategories(
    pagination: PaginationModel,
  ): Observable<ApiResponse<GetAllCategoryResponseDTO>> {
    const requestBody = {
      Pagination: pagination,
    };
    return this.http.post<ApiResponse<GetAllCategoryResponseDTO>>(
      `${this.baseUrl}/get-all-categories`,
      requestBody,
    ).pipe(
      catchError(() => of({ data: undefined, message: '', action: '', statusCode: 0 } as ApiResponse<GetAllCategoryResponseDTO>))
    );
  }

  addCategory(categoryName: string): Observable<ApiResponse<AddCategoryResponseDTO>> {
    const requestBody: AddCategoryRequestDTO = {
      categoryName: categoryName,
    };
    return this.http.post<ApiResponse<AddCategoryResponseDTO>>(
      `${this.baseUrl}/add-category`,
      requestBody,
    );
  }

  deleteCategory(categoryId: string): Observable<ApiResponse<DeleteCategoryResponseDTO>> {
    const requestBody: DeleteCategoryRequestDTO = {
      categoryId: categoryId,
    };

    return this.http.delete<ApiResponse<DeleteCategoryResponseDTO>>(
      `${this.baseUrl}/delete-category`,
      {
        body: requestBody,
      },
    );
  }

  updateCategory(categoryId: string,categoryName: string,): Observable<ApiResponse<EditCategoryResponseDTO>> {
    const requestBody: EditCategoryRequestDTO = {
      categoryId: categoryId,
      categoryName: categoryName,
    };
    return this.http.post<ApiResponse<EditCategoryResponseDTO>>(
      `${this.baseUrl}/edit-category`,
      requestBody,
    );
  }
}
