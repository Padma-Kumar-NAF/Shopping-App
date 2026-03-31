import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/users/apiResponse.model';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private baseUrl = 'https://localhost:7023/Review';

  constructor(private http: HttpClient) {}

  addReview(productId: string, summary: string, reviewPoints: number): Observable<ApiResponse<{ reviewId: string }>> {
    return this.http.post<ApiResponse<{ reviewId: string }>>(`${this.baseUrl}/add-review`, {
      productId,
      summary,
      reviewPoints,
    });
  }

  deleteReview(reviewId: string): Observable<ApiResponse<{ isDeleted: boolean }>> {
    return this.http.post<ApiResponse<{ isDeleted: boolean }>>(`${this.baseUrl}/delete-review`, { reviewId });
  }
}
