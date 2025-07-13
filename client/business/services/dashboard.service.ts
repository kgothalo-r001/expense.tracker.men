import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  DashboardSummary,
  API_ENDPOINTS 
} from '@expense-tracker/abstractions';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly baseUrl = API_ENDPOINTS.DASHBOARD;

  constructor(private http: HttpClient) {}

  getDashboardSummary(
    startDate?: Date, 
    endDate?: Date
  ): Observable<DashboardSummary> {
    const params: any = {};
    
    if (startDate) {
      params.startDate = startDate.toISOString();
    }
    if (endDate) {
      params.endDate = endDate.toISOString();
    }

    return this.http.get<DashboardSummary>(this.baseUrl, { params });
  }
}
