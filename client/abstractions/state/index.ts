export interface TransactionState<T = any> {
  transactions: T[];
  loading: boolean;
  error: string | null;
  selectedTransaction: T | null;
}

export interface CategoryState<T = any> {
  categories: T[];
  loading: boolean;
  error: string | null;
}

export interface DashboardState<T = any> {
  summary: T | null;
  analytics: T | null;
  projection: T | null;
  monthlyTrends: T[] | null;
  categoryTrends: T[] | null;
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

export interface AppState {
  transactions: TransactionState;
  categories: CategoryState;
  dashboard: DashboardState;
}
