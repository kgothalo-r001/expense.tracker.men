import type { TransactionState } from './transaction/transaction.reducer';
import type { CategoryState } from './category/category.reducer';

export interface AppState {
  transactions: TransactionState;
  categories: CategoryState;
}
