import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/users/apiResponse.model';
import { PaginationModel } from '../../models/users/pagination.model';

export interface GetUserOrderDetailsRequestDTO {
  Pagination: PaginationModel;
}

export interface GetUserOrderDetailsResponseDTO {
  items: OrderDetailsResponseDTO[];
}

export interface OrderDetailsResponseDTO {
  orderId: string;
  status: string;
  totalProductsCount: number;
  totalAmount: number;
  deliveryDate: string;
  isRefunded: boolean;
  address: AddressDTO;
  payment: PaymentDTO;
  orderBy: OrderBy;
  items: OrderItemDTO[];
}

export interface OrderBy {
  userEmail: string;
  userName: string;
}

export interface AddressDTO {
  addressId: string;
  addressLine1: string;
  addressLine2: string;
  state: string;
  city: string;
  pincode: string;
}

export interface OrderItemDTO {
  orderDetailsId: string;
  productId: string;
  imagePath: string;
  productName: string;
  quantity: number;
  productPrice: number;
}

export interface PaymentDTO {
  paymentId: string;
  paymentType: string;
}

export interface PlaceOrderItemDTO {
  productId: string;
  productName: string;
  quantity: number;
  productPrice: number;
}

export interface PlaceOrderRequestDTO {
  addressId: string;
  totalProductsCount: number;
  totalAmount: number;
  orderProductdDetails: PlaceOrderItemDTO;
  paymentType: string;
  promoCode: string;
  useWallet: boolean;
}

export interface PlaceOrderResponseDTO {
  isSuccess: boolean;
  orderId: string;
  paymentId: string;
  deliveryDate: string;
  orderDetailsId: string;
  subtotal: number;
  tax: number;
  shipping: number;
  discountPercentage: number;
  discountAmount: number;
  walletUsed: number;
  finalAmount: number;
}

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private baseUrl = 'https://localhost:7023/orders';

  constructor(private http: HttpClient) {}

  getUserOrders(pagination: PaginationModel): Observable<ApiResponse<GetUserOrderDetailsResponseDTO>> {
    const requestBody: GetUserOrderDetailsRequestDTO = { Pagination: pagination };
    return this.http.post<ApiResponse<GetUserOrderDetailsResponseDTO>>(
      `${this.baseUrl}/get-user-orders`,
      requestBody
    );
  }

  cancelOrder(orderId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/cancel-order`, { OrderId: orderId });
  }

  placeOrder(request: PlaceOrderRequestDTO): Observable<ApiResponse<PlaceOrderResponseDTO>> {
    return this.http.post<ApiResponse<PlaceOrderResponseDTO>>(`${this.baseUrl}/place-order`, request);
  }
}
