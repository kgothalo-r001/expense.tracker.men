import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AppTransactionState } from './transaction.reducer';
import { Transaction, TransactionType, TransactionType2 } from '../../auto/autobusinessclient';

export const selectTransactionState = createFeatureSelector<AppTransactionState>('transactions');

export const selectAllTransactions = createSelector(
  selectTransactionState,
  (state) => state.transactions
);

export const selectTransactionLoading = createSelector(
  selectTransactionState,
  (state) => state.loading
);

export const selectTransactionError = createSelector(
  selectTransactionState,
  (state) => state.error
);

export const selectTransactionById = (id: number) => createSelector(
  selectAllTransactions,
  (transactions) => transactions.find((transaction: Transaction) => Number(transaction.id) === id)
);

export const selectFilteredTransactions = (filters: {
  type?: TransactionType | 'ALL';
  categoryId?: string;
  startDate?: Date;
  endDate?: Date;
  searchTerm?: string;
}) => createSelector(
  selectAllTransactions,
  (transactions) => {
    if (!transactions) return [];

    return transactions.filter((transaction: Transaction) => {
      // Type filter
      if (filters.type && filters.type !== 'ALL' && transaction.type !== (filters.type as any)) {
        return false;
      }

      // Category filter
      if (filters.categoryId && transaction.categoryId?.toString() !== filters.categoryId) {
        return false;
      }

      // Date range filter
      if (filters.startDate && transaction.date && new Date(transaction.date) < filters.startDate) {
        return false;
      }
      if (filters.endDate && transaction.date && new Date(transaction.date) > filters.endDate) {
        return false;
      }

      // Search term filter
      if (filters.searchTerm) {
        const searchLower = filters.searchTerm.toLowerCase();
        const matchesDescription = transaction.description?.toLowerCase().includes(searchLower);
        const matchesAmount = transaction.amount?.toString().includes(filters.searchTerm);
        
        if (!matchesDescription && !matchesAmount) {
          return false;
        }
      }

      return true;
    });
  }
);

export const selectTransactionsByType = (type: TransactionType2) => createSelector(
  selectAllTransactions,
  (transactions) => transactions.filter((t: Transaction) => t.type === type)
);

export const selectTransactionsByCategory = (categoryId: string) => createSelector(
  selectAllTransactions,
  (transactions) => transactions.filter((t: Transaction) => t.categoryId === categoryId)
);

export const selectTransactionsByDateRange = (startDate: Date, endDate: Date) => createSelector(
  selectAllTransactions,
  (transactions) => transactions.filter((t: Transaction) => {
    if (!t.date) return false;
    const transactionDate = new Date(t.date);
    return transactionDate >= startDate && transactionDate <= endDate;
  })
);
