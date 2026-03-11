import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AddressModel } from "../models/address.model";
import { PaginationModel } from "../models/pagination.model";

@Injectable({
    providedIn : 'root',
})
export class AddressApiService{
    private baseUrl = 'https://localhost:7023/';

    constructor(private Http : HttpClient){

    }

    GetUserAddresses(pagination : PaginationModel) : Observable<AddressModel>{
        return this.Http.post<AddressModel>(`${this.baseUrl}Address/GetUserAddress`,pagination);
    }
}