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

export interface AppState {
  transactions: TransactionState;
  categories: CategoryState;
}
