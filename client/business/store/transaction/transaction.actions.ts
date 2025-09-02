import { createAction, props } from '@ngrx/store';
import { Transaction, CreateTransactionRequest, UpdateTransactionRequest } from '../../auto/autobusinessclient';

// Load Actions
export const loadTransactions = createAction('[Transaction] Load Transactions');
export const loadTransactionsSuccess = createAction(
  '[Transaction] Load Transactions Success',
  props<{ transactions: Transaction[] }>()
);
export const loadTransactionsFailure = createAction(
  '[Transaction] Load Transactions Failure',
  props<{ error: string }>()
);

export const loadTransactionsSkipped = createAction(
  '[Transaction] Load Transactions Skipped - Already Loaded'
);

// Create Transaction Actions
export const addTransaction = createAction(
  '[Transaction] Add Transaction',
  props<{ transaction: Transaction }>()
);
export const addTransactionSuccess = createAction(
  '[Transaction] Add Transaction Success',
  props<{ transaction: Transaction }>()
);
export const addTransactionFailure = createAction(
  '[Transaction] Add Transaction Failure',
  props<{ error: string }>()
);

// Update Transaction Actions
export const updateTransaction = createAction(
  '[Transaction] Update Transaction',
  props<{ id: string; transaction: Transaction }>()
);
export const updateTransactionSuccess = createAction(
  '[Transaction] Update Transaction Success',
  props<{ transaction: Transaction }>()
);
export const updateTransactionFailure = createAction(
  '[Transaction] Update Transaction Failure',
  props<{ error: string }>()
);

// Delete Transaction Actions
export const deleteTransaction = createAction(
  '[Transaction] Delete Transaction',
  props<{ id: string }>()
);
export const deleteTransactionSuccess = createAction(
  '[Transaction] Delete Transaction Success',
  props<{ id: string }>()
);
export const deleteTransactionFailure = createAction(
  '[Transaction] Delete Transaction Failure',
  props<{ error: string }>()
);
