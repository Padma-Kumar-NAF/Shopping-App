import { Injectable } from '@angular/core';
import { PaginationModel } from '../models/pagination.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private baseUrl = 'https://localhost:7023/Cart';

  constructor(private Http: HttpClient) {
    console.log("Cart Service Constructorrrrrrrrrrrrrrrr")
  }

  GetUserCart(pagination: PaginationModel){
    const body = { Pagination: pagination };
    return this.Http.post(`${this.baseUrl}/GetUserCart`,body)
  }
}
