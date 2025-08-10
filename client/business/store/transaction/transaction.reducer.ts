import { createReducer, on } from '@ngrx/store';
import { Transaction } from '../../auto/autobusinessclient';
import { TransactionState } from '../../../abstractions';
import * as TransactionActions from './transaction.actions';

// Specialize the generic TransactionState with our Transaction type
export type AppTransactionState = TransactionState<Transaction>;

export const initialState: AppTransactionState = {
  transactions: [],
  loading: false,
  error: null,
  selectedTransaction: null
};

export const transactionReducer = createReducer(
  initialState,

  // Load Transactions
  on(TransactionActions.loadTransactions, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(TransactionActions.loadTransactionsSuccess, (state, { transactions }) => ({
    ...state,
    transactions,
    loading: false,
    error: null
  })),

  on(TransactionActions.loadTransactionsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Add Transaction
  on(TransactionActions.addTransaction, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(TransactionActions.addTransactionSuccess, (state, { transaction }) => ({
    ...state,
    transactions: [...state.transactions, transaction],
    loading: false,
    error: null
  })),

  on(TransactionActions.addTransactionFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Update Transaction
  on(TransactionActions.updateTransaction, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(TransactionActions.updateTransactionSuccess, (state, { transaction }) => ({
    ...state,
    transactions: state.transactions.map(t => 
      t.id === transaction.id ? transaction : t
    ),
    loading: false,
    error: null,
    selectedTransaction: state.selectedTransaction?.id === transaction.id ? transaction : state.selectedTransaction
  })),

  on(TransactionActions.updateTransactionFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Delete Transaction
  on(TransactionActions.deleteTransaction, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(TransactionActions.deleteTransactionSuccess, (state, { id }) => ({
    ...state,
    transactions: state.transactions.filter(t => t.id !== id),
    loading: false,
    error: null,
    selectedTransaction: state.selectedTransaction?.id === id ? null : state.selectedTransaction
  })),

  on(TransactionActions.deleteTransactionFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
