import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TransactionState } from '..';
import { Transaction } from '../../../../auto/autoexpensetrackerclient';

export const selectTransactionState = createFeatureSelector<TransactionState>('transactions');

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
