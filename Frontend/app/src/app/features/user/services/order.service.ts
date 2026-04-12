import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/users/apiResponse.model';
import { PaginationModel } from '../../../shared/models/users/pagination.model';
import { CancelOrderResponseDTO, GetUserOrderDetailsRequestDTO, GetUserOrderDetailsResponseDTO, PlaceOrderRequestDTO, PlaceOrderResponseDTO } from '../../../shared/models/users/order.model';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private baseUrl = 'https://localhost:7023/orders';

  constructor(private http: HttpClient) {

  }

  getUserOrders(pagination: PaginationModel): Observable<ApiResponse<GetUserOrderDetailsResponseDTO>> {
    const requestBody: GetUserOrderDetailsRequestDTO = { Pagination: pagination };
    return this.http.post<ApiResponse<GetUserOrderDetailsResponseDTO>>(`${this.baseUrl}/get-user-orders`,requestBody);
  }

  cancelOrder(orderId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<CancelOrderResponseDTO>>(`${this.baseUrl}/cancel-order`, { OrderId: orderId });
  }

  placeOrder(request: PlaceOrderRequestDTO): Observable<ApiResponse<PlaceOrderResponseDTO>> {
    return this.http.post<ApiResponse<PlaceOrderResponseDTO>>(`${this.baseUrl}/place-order`, request);
  }
}
