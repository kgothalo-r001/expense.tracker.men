import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, combineLatest, map } from 'rxjs';
import { 
  DashboardSummary, 
  ExpenseAnalytics, 
  BudgetProjection, 
  MonthlySpending, 
  CategoryTrend 
} from '../../../../auto/autoexpensetrackerclient';
import { DashboardService } from '../../../../../business';
import { MonthlySpendingTrendsComponent } from '../charts/monthly-spending-trends/monthly-spending-trends.component';
import { CategoryTrendsComponent } from '../charts/category-trends/category-trends.component';
import { BudgetProjectionComponent } from '../charts/budget-projection/budget-projection.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MonthlySpendingTrendsComponent,
    CategoryTrendsComponent,
    BudgetProjectionComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.less'
})
export class DashboardComponent implements OnInit {
  dashboardSummary$!: Observable<DashboardSummary>;
  expenseAnalytics$!: Observable<ExpenseAnalytics>;
  budgetProjection$!: Observable<BudgetProjection>;
  monthlySpendingTrends$!: Observable<MonthlySpending[]>;
  categoryTrends$!: Observable<CategoryTrend[]>;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.dashboardSummary$ = this.dashboardService.getDashboardSummary();
    this.expenseAnalytics$ = this.dashboardService.getExpenseAnalytics();
    this.budgetProjection$ = this.dashboardService.getBudgetProjection();
    this.monthlySpendingTrends$ = this.dashboardService.getMonthlySpendingTrends();
    this.categoryTrends$ = this.dashboardService.getCategoryTrends();
  }

  onDateRangeChange(startDate: Date, endDate: Date): void {
    this.dashboardSummary$ = this.dashboardService.getDashboardSummary();
  }
}
