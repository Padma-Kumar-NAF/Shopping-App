import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  ProductDetails,
  GetAllProductsRequestDTO,
  GetAllProductsResponseDTO,
  SearchProductByNameRequestDTO,
  SearchProductByNameResponseDTO,
  SearchProductByIdRequestDTO,
  SearchProductByIdResponseDTO,
} from '../models/users/product.model';
import { ApiResponse } from '../models/users/apiResponse.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private baseUrl = 'https://localhost:7023/Products';
  private http = inject(HttpClient);

  getAllProducts(pageSize = 20, pageNumber = 1): Observable<ProductDetails[]> {
    const body: GetAllProductsRequestDTO = {
      pagination: { pageSize, pageNumber },
    };
    return this.http
      .post<ApiResponse<GetAllProductsResponseDTO>>(`${this.baseUrl}/get-products`, body)
      .pipe(map((res) => res.data?.productList ?? []));
  }

  searchProducts(productName: string): Observable<ProductDetails[]> {
    const body: SearchProductByNameRequestDTO = { productName };
    return this.http
      .post<ApiResponse<SearchProductByNameResponseDTO>>(`${this.baseUrl}/search-product`, body)
      .pipe(map((res) => res.data?.productsList ?? []));
  }

  getProductById(productId: string): Observable<SearchProductByIdResponseDTO | null> {
    const body: SearchProductByIdRequestDTO = { productId };
    return this.http
      .post<ApiResponse<SearchProductByIdResponseDTO>>(`${this.baseUrl}/get-product-by-id`, body)
      .pipe(map((res) => res.data ?? null));
  }

  getProductsByCategory(categoryName: string, pageSize = 20, pageNumber = 1): Observable<ProductDetails[]> {
    // Backend doesn't have a category filter endpoint — fetch all and filter client-side
    return this.getAllProducts(pageSize, pageNumber).pipe(
      map((products) =>
        products.filter(
          (p) => p.categoryName.toLowerCase() === categoryName.toLowerCase()
        )
      )
    );
  }
}
