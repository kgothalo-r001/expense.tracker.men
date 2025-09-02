import { createFeatureSelector, createSelector } from '@ngrx/store';
import { DashboardState } from './dashboard.reducer';

export const selectDashboardState = createFeatureSelector<DashboardState>('dashboard');

// Data selectors
export const selectDashboardSummary = createSelector(
  selectDashboardState,
  (state) => state.summary
);

export const selectExpenseAnalytics = createSelector(
  selectDashboardState,
  (state) => state.analytics
);

export const selectBudgetProjection = createSelector(
  selectDashboardState,
  (state) => state.projection
);

export const selectMonthlySpendingTrends = createSelector(
  selectDashboardState,
  (state) => state.monthlyTrends
);

export const selectCategoryTrends = createSelector(
  selectDashboardState,
  (state) => state.categoryTrends
);

// Loading selectors
export const selectDashboardLoading = createSelector(
  selectDashboardState,
  (state) => state.loading
);

export const selectDashboardSummaryLoading = createSelector(
  selectDashboardState,
  (state) => state.loading.summary
);

export const selectExpenseAnalyticsLoading = createSelector(
  selectDashboardState,
  (state) => state.loading.analytics
);

export const selectBudgetProjectionLoading = createSelector(
  selectDashboardState,
  (state) => state.loading.projection
);

export const selectMonthlyTrendsLoading = createSelector(
  selectDashboardState,
  (state) => state.loading.monthlyTrends
);

export const selectCategoryTrendsLoading = createSelector(
  selectDashboardState,
  (state) => state.loading.categoryTrends
);

export const selectAnyDashboardLoading = createSelector(
  selectDashboardLoading,
  (loading) => Object.values(loading).some(isLoading => isLoading)
);

// Error selectors
export const selectDashboardErrors = createSelector(
  selectDashboardState,
  (state) => state.errors
);

export const selectDashboardSummaryError = createSelector(
  selectDashboardState,
  (state) => state.errors.summary
);

// Last updated selectors
export const selectDashboardLastUpdated = createSelector(
  selectDashboardState,
  (state) => state.lastUpdated
);

// Combined selectors
export const selectAllDashboardData = createSelector(
  selectDashboardSummary,
  selectExpenseAnalytics,
  selectBudgetProjection,
  selectMonthlySpendingTrends,
  selectCategoryTrends,
  (summary, analytics, projection, monthlyTrends, categoryTrends) => ({
    summary,
    analytics,
    projection,
    monthlyTrends,
    categoryTrends
  })
);

export const selectDashboardDataExists = createSelector(
  selectAllDashboardData,
  (data) => !!(data.summary && data.analytics && data.projection && data.monthlyTrends && data.categoryTrends)
);
