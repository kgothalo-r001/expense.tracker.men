import { createAction, props } from '@ngrx/store';
import { 
  DashboardSummary, 
  ExpenseAnalytics, 
  BudgetProjection, 
  MonthlySpending, 
  CategoryTrend 
} from '../../auto/autobusinessclient';

// Dashboard Summary Actions
export const loadDashboardSummary = createAction('[Dashboard] Load Summary');
export const loadDashboardSummarySuccess = createAction(
  '[Dashboard] Load Summary Success',
  props<{ summary: DashboardSummary }>()
);
export const loadDashboardSummaryFailure = createAction(
  '[Dashboard] Load Summary Failure',
  props<{ error: string }>()
);

// Expense Analytics Actions
export const loadExpenseAnalytics = createAction('[Dashboard] Load Expense Analytics');
export const loadExpenseAnalyticsSuccess = createAction(
  '[Dashboard] Load Expense Analytics Success',
  props<{ analytics: ExpenseAnalytics }>()
);
export const loadExpenseAnalyticsFailure = createAction(
  '[Dashboard] Load Expense Analytics Failure',
  props<{ error: string }>()
);

// Budget Projection Actions
export const loadBudgetProjection = createAction('[Dashboard] Load Budget Projection');
export const loadBudgetProjectionSuccess = createAction(
  '[Dashboard] Load Budget Projection Success',
  props<{ projection: BudgetProjection }>()
);
export const loadBudgetProjectionFailure = createAction(
  '[Dashboard] Load Budget Projection Failure',
  props<{ error: string }>()
);

// Monthly Spending Trends Actions
export const loadMonthlySpendingTrends = createAction('[Dashboard] Load Monthly Spending Trends');
export const loadMonthlySpendingTrendsSuccess = createAction(
  '[Dashboard] Load Monthly Spending Trends Success',
  props<{ trends: MonthlySpending[] }>()
);
export const loadMonthlySpendingTrendsFailure = createAction(
  '[Dashboard] Load Monthly Spending Trends Failure',
  props<{ error: string }>()
);

// Category Trends Actions
export const loadCategoryTrends = createAction('[Dashboard] Load Category Trends');
export const loadCategoryTrendsSuccess = createAction(
  '[Dashboard] Load Category Trends Success',
  props<{ trends: CategoryTrend[] }>()
);
export const loadCategoryTrendsFailure = createAction(
  '[Dashboard] Load Category Trends Failure',
  props<{ error: string }>()
);

// Load All Dashboard Data Action
export const loadAllDashboardData = createAction('[Dashboard] Load All Data');

// Refresh Dashboard Data Action (forces reload)
export const refreshDashboardData = createAction('[Dashboard] Refresh All Data');

// Dashboard Data Skipped Action
export const loadAllDashboardDataSkipped = createAction(
  '[Dashboard] Load All Data Skipped - Cache Valid'
);
