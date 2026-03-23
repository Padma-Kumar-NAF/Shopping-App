import { Injectable } from '@angular/core';
import { PaginationModel } from '../models/users/pagination.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/users/apiResponse.model';
import { GetCartResponseDTO, RemoveAllFromCartResponseDTO, RemoveFromCartRequestDTO, RemoveFromCartResponseDTO } from '../models/users/cart.model';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private baseUrl = 'https://localhost:7023/Cart';

  constructor(private Http: HttpClient) {
    console.log("Cart Service Constructorrrrrrrrrrrrrrrr")
  }

  GetUserCart(pagination: PaginationModel) : Observable<ApiResponse<GetCartResponseDTO>>{
    const body = { Pagination: pagination };
    return this.Http.post<ApiResponse<GetCartResponseDTO>>(`${this.baseUrl}/get-user-cart`,body)
  }

  removeAllFromCart() : Observable<ApiResponse<RemoveAllFromCartResponseDTO>>{
    return this.Http.post<ApiResponse<RemoveAllFromCartResponseDTO>>(`${this.baseUrl}/remove-all-from-cart`,{})
  }

  removeFromCart(request : RemoveFromCartRequestDTO) : Observable<ApiResponse<RemoveFromCartResponseDTO>>{
    return this.Http.delete<ApiResponse<RemoveFromCartResponseDTO>>(`${this.baseUrl}/remove-from-cart`,{
      body : request
    });
  }
}
