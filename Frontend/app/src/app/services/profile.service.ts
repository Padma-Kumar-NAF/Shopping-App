import { HttpClient } from '@angular/common/http';
import { Injectable, INJECTOR } from '@angular/core';
import { Observable } from 'rxjs';
import { UserProfile } from '../models/profile.model';

@Injectable({
    providedIn : 'root',
})
export class ProfileApiService{
    private baseUrl = 'https://localhost:7023/';
    constructor(private Http : HttpClient){

    }

    GetUserProfile() : Observable<UserProfile> {
        return this.Http.get<UserProfile>(`${this.baseUrl}User/GetUserById`);
    }
}