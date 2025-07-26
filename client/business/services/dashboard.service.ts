import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  IClient,
  DashboardSummary,
  ExpenseAnalytics,
  BudgetProjection,
  Tag,
  CreateTagRequest,
  MonthlySpending,
  CategoryTrend
} from '../auto/autobusinessclient';
import { ICLIENT_TOKEN } from '../index';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(@Inject(ICLIENT_TOKEN) private client: IClient) {}

  getDashboardSummary(startDate?: Date, endDate?: Date): Observable<DashboardSummary> {
    return this.client.getDashboardSummary(startDate, endDate);
  }

  getExpenseAnalytics(monthsBack?: number): Observable<ExpenseAnalytics> {
    return this.client.getExpenseAnalytics(monthsBack);
  }

  getBudgetProjection(): Observable<BudgetProjection> {
    return this.client.getBudgetProjection2();
  }

  getTags(): Observable<Tag[]> {
    return this.client.getTags();
  }

  getTag(id: string): Observable<Tag> {
    return this.client.getTag(id);
  }

  createTag(tag: CreateTagRequest): Observable<Tag> {
    return this.client.createTag(tag);
  }

  deleteTag(id: string): Observable<void> {
    return this.client.deleteTag(id);
  }

  getPopularTags(limit?: number): Observable<Tag[]> {
    return this.client.getPopularTags(limit);
  }

  getMonthlySpendingTrends(monthsBack?: number): Observable<MonthlySpending[]> {
    return this.client.getMonthlySpendingTrends(monthsBack);
  }

  getCategoryTrends(): Observable<CategoryTrend[]> {
    return this.client.getCategoryTrends();
  }
}
