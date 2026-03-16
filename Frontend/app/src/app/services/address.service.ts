import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AddressDTO, AddressModel } from "../models/address.model";
import { PaginationModel } from "../models/pagination.model";

@Injectable({
    providedIn : 'root',
})
export class AddressApiService{
    private baseUrl = 'https://localhost:7023/Address';

    constructor(private Http : HttpClient){

    }

    GetUserAddresses(pagination : PaginationModel) : Observable<AddressModel>{
        return this.Http.post<AddressModel>(`${this.baseUrl}/GetUserAddress`,pagination);
    }

    AddNewAddress(newAddress : AddressDTO) : Observable<AddressDTO> {
        return this.Http.post<AddressDTO>(`${this.baseUrl}/CreateAddress`,newAddress);
    }
}