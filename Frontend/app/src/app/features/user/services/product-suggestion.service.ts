import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse } from '../../../shared/models/users/apiResponse.model';

export interface ProductSuggestionDTO {
  name: string;
}

@Injectable({ providedIn: 'root' })
export class ProductSuggestionService {
  private baseUrl = 'https://localhost:7023/Products';
  private http = inject(HttpClient);

  getSuggestions(query: string): Observable<string[]> {
    const params = new HttpParams().set('query', query);
    return this.http
      .get<ApiResponse<ProductSuggestionDTO[]>>(`${this.baseUrl}/suggestions`, { params })
      .pipe(
        map((res) => (res.data ?? []).map((s) => s.name)),
        catchError(() => of([])),
      );
  }
}
