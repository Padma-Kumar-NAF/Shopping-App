import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/users/apiResponse.model';

export interface WishListItemsDTO {
  wishListItemsId: string;
  productId: string;
  productName: string;
  productImage: string;
}

export interface WishListDTO {
  wishListId: string;
  wishListName: string;
  wishListItems: WishListItemsDTO[];
}

export interface GetUserWishListResponseDTO {
  wishList: WishListDTO[];
}

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private baseUrl = 'https://localhost:7023/api/wishlist';

  constructor(private http: HttpClient) {}

  getUserWishlists(pageSize = 20, pageNumber = 1): Observable<ApiResponse<GetUserWishListResponseDTO>> {
    return this.http.post<ApiResponse<GetUserWishListResponseDTO>>(`${this.baseUrl}/user-wishlist`, {
      pagination: { pageSize, pageNumber },
    });
  }

  createWishlist(wishListName: string): Observable<ApiResponse<{ isCreated: boolean }>> {
    return this.http.post<ApiResponse<{ isCreated: boolean }>>(`${this.baseUrl}/create`, { wishListName });
  }

  addProduct(wishListId: string, productId: string): Observable<ApiResponse<{ isSuccess: boolean }>> {
    return this.http.post<ApiResponse<{ isSuccess: boolean }>>(`${this.baseUrl}/add-product`, { wishListId, productId });
  }

  removeProduct(wishListId: string, productId: string): Observable<ApiResponse<{ isRemoved: boolean }>> {
    return this.http.delete<ApiResponse<{ isRemoved: boolean }>>(`${this.baseUrl}/remove-product`, {
      body: { wishListId, productId },
    });
  }

  deleteWishlist(wishListId: string): Observable<ApiResponse<{ isDeleted: boolean }>> {
    return this.http.delete<ApiResponse<{ isDeleted: boolean }>>(`${this.baseUrl}/delete`, {
      body: { wishListId },
    });
  }
}
