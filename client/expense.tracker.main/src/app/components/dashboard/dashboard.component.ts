import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, combineLatest, map } from 'rxjs';
import { 
  DashboardSummary, 
  ExpenseAnalytics, 
  BudgetProjection, 
  MonthlySpending, 
  CategoryTrend 
} from '@business/auto';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.less'
})
export class DashboardComponent implements OnInit {
  dashboardSummary$!: Observable<DashboardSummary>;
  expenseAnalytics$!: Observable<ExpenseAnalytics>;
  budgetProjection$!: Observable<BudgetProjection>;
  monthlySpendingTrends$!: Observable<MonthlySpending[]>;
  categoryTrends$!: Observable<CategoryTrend[]>;

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
    const today = new Date();

    this.dashboardSummary$ = this.apiService.getDashboardSummary(thirtyDaysAgo, today);
    this.expenseAnalytics$ = this.apiService.getExpenseAnalytics(12);
    this.budgetProjection$ = this.apiService.getBudgetProjection();
    this.monthlySpendingTrends$ = this.apiService.getMonthlySpendingTrends(6);
    this.categoryTrends$ = this.apiService.getCategoryTrends();
  }

  onDateRangeChange(startDate: Date, endDate: Date): void {
    this.dashboardSummary$ = this.apiService.getDashboardSummary(startDate, endDate);
  }
}
