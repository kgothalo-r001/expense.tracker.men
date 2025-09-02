import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import { 
  DashboardSummary, 
  ExpenseAnalytics, 
  BudgetProjection, 
  MonthlySpending, 
  CategoryTrend 
} from '../../../../auto/autoexpensetrackerclient';
import { 
  DashboardActions, 
  AppState,
  selectDashboardSummary,
  selectExpenseAnalytics,
  selectBudgetProjection,
  selectMonthlySpendingTrends,
  selectCategoryTrends,
  selectAnyDashboardLoading,
  selectDashboardErrors
} from '../../../../../business';
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
  dashboardSummary$: Observable<DashboardSummary | null>;
  expenseAnalytics$: Observable<ExpenseAnalytics | null>;
  budgetProjection$: Observable<BudgetProjection | null>;
  monthlySpendingTrends$: Observable<MonthlySpending[] | null>;
  categoryTrends$: Observable<CategoryTrend[] | null>;
  isLoading$: Observable<boolean>;
  errors$: Observable<any>;

  constructor(private store: Store<AppState>) {
    this.dashboardSummary$ = this.store.select(selectDashboardSummary);
    this.expenseAnalytics$ = this.store.select(selectExpenseAnalytics);
    this.budgetProjection$ = this.store.select(selectBudgetProjection);
    this.monthlySpendingTrends$ = this.store.select(selectMonthlySpendingTrends);
    this.categoryTrends$ = this.store.select(selectCategoryTrends);
    this.isLoading$ = this.store.select(selectAnyDashboardLoading);
    this.errors$ = this.store.select(selectDashboardErrors);
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.store.dispatch(DashboardActions.loadAllDashboardData());
  }

  onDateRangeChange(startDate: Date, endDate: Date): void {
    this.store.dispatch(DashboardActions.refreshDashboardData());
  }

  onRefresh(): void {
    this.store.dispatch(DashboardActions.refreshDashboardData());
  }
}
