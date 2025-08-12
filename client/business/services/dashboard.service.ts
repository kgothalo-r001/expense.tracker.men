import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  IClient,
  DashboardSummary,
  ExpenseAnalytics,
  BudgetProjection,
  MonthlySpending,
  CategoryTrend
} from '../auto/autobusinessclient';
import { API_CLIENT } from '../config';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(@Inject(API_CLIENT) private client: IClient) {}

  getDashboardSummary(): Observable<DashboardSummary> {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 30);
    return this.client.getDashboardSummary(startDate, endDate);
  }

  getExpenseAnalytics(): Observable<ExpenseAnalytics> {
    return this.client.getExpenseAnalytics(6);
  }

  getBudgetProjection(): Observable<BudgetProjection> {
    return this.client.getBudgetProjection();
  }

  getMonthlySpendingTrends(): Observable<MonthlySpending[]> {
    return this.client.getMonthlySpendingTrends(12);
  }

  getCategoryTrends(): Observable<CategoryTrend[]> {
    return this.client.getCategoryTrends();
  }
}
