import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AppTransactionState } from './transaction.reducer';
import { Transaction } from '../../auto/autobusinessclient';

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
