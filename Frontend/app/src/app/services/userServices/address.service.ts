import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AddressDTO,
  AddressModel,
  DeleteAddressRequestDTO,
  DeleteAddressResponseDTO,
  NewAddressResponseDTO,
  UpdateAddressResponseDTO,
} from '../../models/users/address.model';
import { PaginationModel } from '../../models/users/pagination.model';
import { ApiResponse } from '../../models/users/apiResponse.model';

@Injectable({
  providedIn: 'root',
})
export class AddressApiService {
  private baseUrl = 'https://localhost:7023/Address';

  constructor(private Http: HttpClient) {
    console.log("Address Service Constructorrrrrrrrrrrrrrrr")
  }

  GetUserAddresses(pagination: PaginationModel): Observable<ApiResponse<AddressModel>> {
    const body = { Pagination: pagination };
    return this.Http.post<ApiResponse<AddressModel>>(`${this.baseUrl}/get-address`, body);
  }

  AddNewAddress(newAddress: AddressDTO): Observable<ApiResponse<NewAddressResponseDTO>> {
    return this.Http.put<ApiResponse<NewAddressResponseDTO>>(`${this.baseUrl}/create-address`, newAddress);
  }

  DeleteAddress(deleteAddress: DeleteAddressRequestDTO): Observable<ApiResponse<DeleteAddressResponseDTO>> {
    return this.Http.delete<ApiResponse<DeleteAddressResponseDTO>>(`${this.baseUrl}/delete-address`, {
      body: deleteAddress,
    });
  }

  UpdateAddress(updatedAddress: AddressDTO): Observable<ApiResponse<UpdateAddressResponseDTO>> {
    return this.Http.post<ApiResponse<UpdateAddressResponseDTO>>(
      `${this.baseUrl}/edit-address`,
      updatedAddress,
    );
  }
}
