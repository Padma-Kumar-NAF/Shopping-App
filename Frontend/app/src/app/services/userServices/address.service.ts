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

@Injectable({
  providedIn: 'root',
})
export class AddressApiService {
  private baseUrl = 'https://localhost:7023/Address';

  constructor(private Http: HttpClient) {
    console.log("Address Service Constructorrrrrrrrrrrrrrrr")
  }

  GetUserAddresses(pagination: PaginationModel): Observable<AddressModel> {
    const body = { Pagination: pagination };
    return this.Http.post<AddressModel>(`${this.baseUrl}/GetUserAddress`, body);
  }

  AddNewAddress(newAddress: AddressDTO): Observable<NewAddressResponseDTO> {
    return this.Http.put<NewAddressResponseDTO>(`${this.baseUrl}/CreateAddress`, newAddress);
  }

  DeleteAddress(deleteAddress: DeleteAddressRequestDTO): Observable<DeleteAddressResponseDTO> {
    return this.Http.delete<DeleteAddressResponseDTO>(`${this.baseUrl}/DeleteUserAddress`, {
      body: deleteAddress,
    });
  }

  UpdateAddress(updatedAddress: AddressDTO): Observable<UpdateAddressResponseDTO> {
    return this.Http.post<UpdateAddressResponseDTO>(
      `${this.baseUrl}/EditUserAddress`,
      updatedAddress,
    );
  }
}
