import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationModel } from '../../../shared/models/users/pagination.model';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/users/apiResponse.model';
import {
  GetAllOrderResponseDTO,
  OrderRefundRequestDTO,
  OrderRefundResponseDTO,
  UpdateOrderRequestDTO,
  UpdateOrderResponseDTO,
} from '../../../shared/models/admin/orders.model';

@Injectable({
  providedIn: 'root',
})
export class AdminOrderService {
  private baseUrl = 'https://localhost:7023/orders';
  constructor(private http: HttpClient) {}

  getAllOrders(pagination: PaginationModel): Observable<ApiResponse<GetAllOrderResponseDTO>> {
    const requestBody = {
      Pagination: pagination,
    };
    return this.http.post<ApiResponse<GetAllOrderResponseDTO>>(`${this.baseUrl}/get-all-orders`,requestBody,);
  }

  updateOrder(orderId: string, status: string): Observable<ApiResponse<UpdateOrderResponseDTO>> {
    const updateOrderRequestDTO: UpdateOrderRequestDTO = {
      OrderId: orderId,
      OrderStatus: status,
    };
    return this.http.post<ApiResponse<UpdateOrderResponseDTO>>(`${this.baseUrl}/update-order-status`,updateOrderRequestDTO,);
  }

  refundOrder(request: OrderRefundRequestDTO): Observable<ApiResponse<OrderRefundResponseDTO>> {
    return this.http.post<ApiResponse<OrderRefundResponseDTO>>(`${this.baseUrl}/refund-order`,request,);
  }
}
