import { Injectable } from '@angular/core';
import { PaginationModel } from '../../../shared/models/users/pagination.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/users/apiResponse.model';
import {
  AddToCartRequestDTO, AddToCartResponseDTO, GetCartResponseDTO, OrderAllFromCartRequestDTO, OrderAllFromCartResponseDTO,
  RemoveAllFromCartResponseDTO, RemoveFromCartRequestDTO, RemoveFromCartResponseDTO, UpdateUserCartRequestDTO, UpdateUserCartResponseDTO,
} from '../../../shared/models/users/cart.model';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private baseUrl = 'https://localhost:7023/Cart';

  constructor(private Http: HttpClient) { }

  GetUserCart(pagination: PaginationModel): Observable<ApiResponse<GetCartResponseDTO>> {
    const body = { Pagination: pagination };
    return this.Http.post<ApiResponse<GetCartResponseDTO>>(`${this.baseUrl}/get-user-cart`, body);
  }

  addToCart(request: AddToCartRequestDTO): Observable<ApiResponse<AddToCartResponseDTO>> {
    return this.Http.post<ApiResponse<AddToCartResponseDTO>>(`${this.baseUrl}/add-to-cart`, request);
  }

  updateCart(request: UpdateUserCartRequestDTO): Observable<ApiResponse<UpdateUserCartResponseDTO>> {
    return this.Http.post<ApiResponse<UpdateUserCartResponseDTO>>(`${this.baseUrl}/update-user-cart`, request);
  }

  orderAllFromCart(request: OrderAllFromCartRequestDTO): Observable<ApiResponse<OrderAllFromCartResponseDTO>> {
    return this.Http.post<ApiResponse<OrderAllFromCartResponseDTO>>(`${this.baseUrl}/order-all-from-cart`, request);
  }

  removeAllFromCart(): Observable<ApiResponse<RemoveAllFromCartResponseDTO>> {
    return this.Http.post<ApiResponse<RemoveAllFromCartResponseDTO>>(`${this.baseUrl}/remove-all-from-cart`, {});
  }

  removeFromCart(request: RemoveFromCartRequestDTO): Observable<ApiResponse<RemoveFromCartResponseDTO>> {
    return this.Http.delete<ApiResponse<RemoveFromCartResponseDTO>>(`${this.baseUrl}/remove-from-cart`, { body: request });
  }
}
