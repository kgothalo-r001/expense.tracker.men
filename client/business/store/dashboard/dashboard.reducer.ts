import { createReducer, on } from '@ngrx/store';
import { 
  DashboardSummary, 
  ExpenseAnalytics, 
  BudgetProjection, 
  MonthlySpending, 
  CategoryTrend 
} from '../../auto/autobusinessclient';
import * as DashboardActions from './dashboard.actions';

export interface DashboardState {
  summary: DashboardSummary | null;
  analytics: ExpenseAnalytics | null;
  projection: BudgetProjection | null;
  monthlyTrends: MonthlySpending[] | null;
  categoryTrends: CategoryTrend[] | null;
  loading: {
    summary: boolean;
    analytics: boolean;
    projection: boolean;
    monthlyTrends: boolean;
    categoryTrends: boolean;
  };
  errors: {
    summary: string | null;
    analytics: string | null;
    projection: string | null;
    monthlyTrends: string | null;
    categoryTrends: string | null;
  };
  lastUpdated: {
    summary: Date | null;
    analytics: Date | null;
    projection: Date | null;
    monthlyTrends: Date | null;
    categoryTrends: Date | null;
  };
}

export const initialState: DashboardState = {
  summary: null,
  analytics: null,
  projection: null,
  monthlyTrends: null,
  categoryTrends: null,
  loading: {
    summary: false,
    analytics: false,
    projection: false,
    monthlyTrends: false,
    categoryTrends: false,
  },
  errors: {
    summary: null,
    analytics: null,
    projection: null,
    monthlyTrends: null,
    categoryTrends: null,
  },
  lastUpdated: {
    summary: null,
    analytics: null,
    projection: null,
    monthlyTrends: null,
    categoryTrends: null,
  }
};

export const dashboardReducer = createReducer(
  initialState,

  // Load All Dashboard Data
  on(DashboardActions.loadAllDashboardData, (state) => ({
    ...state,
    loading: {
      summary: true,
      analytics: true,
      projection: true,
      monthlyTrends: true,
      categoryTrends: true,
    },
    errors: {
      summary: null,
      analytics: null,
      projection: null,
      monthlyTrends: null,
      categoryTrends: null,
    }
  })),

  // Refresh Dashboard Data
  on(DashboardActions.refreshDashboardData, (state) => ({
    ...state,
    loading: {
      summary: true,
      analytics: true,
      projection: true,
      monthlyTrends: true,
      categoryTrends: true,
    },
    errors: {
      summary: null,
      analytics: null,
      projection: null,
      monthlyTrends: null,
      categoryTrends: null,
    }
  })),

  // Load All Dashboard Data Skipped (Cache Valid)
  on(DashboardActions.loadAllDashboardDataSkipped, (state) => ({
    ...state,
    loading: {
      summary: false,
      analytics: false,
      projection: false,
      monthlyTrends: false,
      categoryTrends: false,
    }
  })),

  // Dashboard Summary
  on(DashboardActions.loadDashboardSummary, (state) => ({
    ...state,
    loading: { ...state.loading, summary: true },
    errors: { ...state.errors, summary: null }
  })),

  on(DashboardActions.loadDashboardSummarySuccess, (state, { summary }) => ({
    ...state,
    summary,
    loading: { ...state.loading, summary: false },
    errors: { ...state.errors, summary: null },
    lastUpdated: { ...state.lastUpdated, summary: new Date() }
  })),

  on(DashboardActions.loadDashboardSummaryFailure, (state, { error }) => ({
    ...state,
    loading: { ...state.loading, summary: false },
    errors: { ...state.errors, summary: error }
  })),

  // Expense Analytics
  on(DashboardActions.loadExpenseAnalytics, (state) => ({
    ...state,
    loading: { ...state.loading, analytics: true },
    errors: { ...state.errors, analytics: null }
  })),

  on(DashboardActions.loadExpenseAnalyticsSuccess, (state, { analytics }) => ({
    ...state,
    analytics,
    loading: { ...state.loading, analytics: false },
    errors: { ...state.errors, analytics: null },
    lastUpdated: { ...state.lastUpdated, analytics: new Date() }
  })),

  on(DashboardActions.loadExpenseAnalyticsFailure, (state, { error }) => ({
    ...state,
    loading: { ...state.loading, analytics: false },
    errors: { ...state.errors, analytics: error }
  })),

  // Budget Projection
  on(DashboardActions.loadBudgetProjection, (state) => ({
    ...state,
    loading: { ...state.loading, projection: true },
    errors: { ...state.errors, projection: null }
  })),

  on(DashboardActions.loadBudgetProjectionSuccess, (state, { projection }) => ({
    ...state,
    projection,
    loading: { ...state.loading, projection: false },
    errors: { ...state.errors, projection: null },
    lastUpdated: { ...state.lastUpdated, projection: new Date() }
  })),

  on(DashboardActions.loadBudgetProjectionFailure, (state, { error }) => ({
    ...state,
    loading: { ...state.loading, projection: false },
    errors: { ...state.errors, projection: error }
  })),

  // Monthly Spending Trends
  on(DashboardActions.loadMonthlySpendingTrends, (state) => ({
    ...state,
    loading: { ...state.loading, monthlyTrends: true },
    errors: { ...state.errors, monthlyTrends: null }
  })),

  on(DashboardActions.loadMonthlySpendingTrendsSuccess, (state, { trends }) => ({
    ...state,
    monthlyTrends: trends,
    loading: { ...state.loading, monthlyTrends: false },
    errors: { ...state.errors, monthlyTrends: null },
    lastUpdated: { ...state.lastUpdated, monthlyTrends: new Date() }
  })),

  on(DashboardActions.loadMonthlySpendingTrendsFailure, (state, { error }) => ({
    ...state,
    loading: { ...state.loading, monthlyTrends: false },
    errors: { ...state.errors, monthlyTrends: error }
  })),

  // Category Trends
  on(DashboardActions.loadCategoryTrends, (state) => ({
    ...state,
    loading: { ...state.loading, categoryTrends: true },
    errors: { ...state.errors, categoryTrends: null }
  })),

  on(DashboardActions.loadCategoryTrendsSuccess, (state, { trends }) => ({
    ...state,
    categoryTrends: trends,
    loading: { ...state.loading, categoryTrends: false },
    errors: { ...state.errors, categoryTrends: null },
    lastUpdated: { ...state.lastUpdated, categoryTrends: new Date() }
  })),

  on(DashboardActions.loadCategoryTrendsFailure, (state, { error }) => ({
    ...state,
    loading: { ...state.loading, categoryTrends: false },
    errors: { ...state.errors, categoryTrends: error }
  }))
);
