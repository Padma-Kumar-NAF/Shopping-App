import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../shared/models/users/apiResponse.model';

export interface GetWalletAmountResponseDTO {
  walletBalance: number;
}

@Injectable({ providedIn: 'root' })
export class WalletService {
  private baseUrl = 'https://localhost:7023/api/Wallet';
  private http = inject(HttpClient);

  getWalletBalance(): Observable<ApiResponse<GetWalletAmountResponseDTO>> {
    return this.http.get<ApiResponse<GetWalletAmountResponseDTO>>(`${this.baseUrl}/balance`);
  }
}
