import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/admin/apiResponse.model';
import { AddNewProductRequestDTO, AddNewProductResponseDTO, GetAllProductsResponseDTO, UpdateProductRequestDTO, UpdateProductResponseDTO } from '../../../shared/models/admin/products.model';
import { PaginationModel } from '../../../shared/models/admin/pagination.model';

@Injectable({
  providedIn: 'root',
})
export class AdminProductService {
  private baseUrl = 'https://localhost:7023/Products';
  constructor(private http: HttpClient) {}

  getAllProducts(pagination: PaginationModel): Observable<ApiResponse<GetAllProductsResponseDTO>> {
    const requestBody = {
      Pagination: pagination,
    };
    return this.http.post<ApiResponse<GetAllProductsResponseDTO>>(
      `${this.baseUrl}/get-products`,
      requestBody,
    );
  }

  addProduct(
    payload: AddNewProductRequestDTO,
  ): Observable<ApiResponse<AddNewProductResponseDTO>> {
    return this.http.post<ApiResponse<AddNewProductResponseDTO>>(
      `${this.baseUrl}/add-product`,
      payload,
    );
  }
 
  updateProduct(
    payload: UpdateProductRequestDTO,
  ): Observable<ApiResponse<UpdateProductResponseDTO>> {
    return this.http.post<ApiResponse<UpdateProductResponseDTO>>(
      `${this.baseUrl}/update-product`,
      payload,
    );
  }
 
  deleteProduct(productId: string): Observable<ApiResponse<{ isDeleted: boolean }>> {
    return this.http.post<ApiResponse<{ isDeleted: boolean }>>(
      `${this.baseUrl}/delete-product`,
      { productId },
    );
  }
}
