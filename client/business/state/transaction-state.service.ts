import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest, map, catchError, of, switchMap, tap } from 'rxjs';
import { Transaction, LoadingState, LOADING_STATES } from '@expense-tracker/abstractions';
import { TransactionService } from '../services';

interface TransactionState {
  transactions: Transaction[];
  selectedTransaction: Transaction | null;
  loadingState: LoadingState;
  error: string | null;
  filters: TransactionFilters;
  total: number;
}

interface TransactionFilters {
  page: number;
  pageSize: number;
  categoryId?: string;
  type?: string;
  startDate?: Date;
  endDate?: Date;
  tags?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class TransactionStateService {
  private readonly initialState: TransactionState = {
    transactions: [],
    selectedTransaction: null,
    loadingState: LOADING_STATES.IDLE,
    error: null,
    filters: {
      page: 1,
      pageSize: 20
    },
    total: 0
  };

  private readonly state$ = new BehaviorSubject<TransactionState>(this.initialState);

  // Selectors
  readonly transactions$ = this.state$.pipe(map(state => state.transactions));
  readonly selectedTransaction$ = this.state$.pipe(map(state => state.selectedTransaction));
  readonly loadingState$ = this.state$.pipe(map(state => state.loadingState));
  readonly error$ = this.state$.pipe(map(state => state.error));
  readonly filters$ = this.state$.pipe(map(state => state.filters));
  readonly total$ = this.state$.pipe(map(state => state.total));
  readonly isLoading$ = this.loadingState$.pipe(map(state => state === LOADING_STATES.LOADING));

  constructor(private transactionService: TransactionService) {}

  // Actions
  loadTransactions(): void {
    this.updateState({ loadingState: LOADING_STATES.LOADING, error: null });
    
    const filters = this.state$.value.filters;
    
    this.transactionService.getTransactions(filters).pipe(
      tap(response => {
        this.updateState({
          transactions: response.transactions,
          total: response.total,
          loadingState: LOADING_STATES.SUCCESS,
          error: null
        });
      }),
      catchError(error => {
        this.updateState({
          loadingState: LOADING_STATES.ERROR,
          error: error.message || 'Failed to load transactions'
        });
        return of(null);
      })
    ).subscribe();
  }

  setFilters(filters: Partial<TransactionFilters>): void {
    const currentFilters = this.state$.value.filters;
    const newFilters = { ...currentFilters, ...filters };
    this.updateState({ filters: newFilters });
    this.loadTransactions();
  }

  selectTransaction(transaction: Transaction | null): void {
    this.updateState({ selectedTransaction: transaction });
  }

  addTransaction(transaction: Transaction): void {
    const currentTransactions = this.state$.value.transactions;
    this.updateState({
      transactions: [transaction, ...currentTransactions]
    });
  }

  updateTransaction(updatedTransaction: Transaction): void {
    const currentTransactions = this.state$.value.transactions;
    const updatedTransactions = currentTransactions.map(t => 
      t.id === updatedTransaction.id ? updatedTransaction : t
    );
    this.updateState({
      transactions: updatedTransactions,
      selectedTransaction: this.state$.value.selectedTransaction?.id === updatedTransaction.id 
        ? updatedTransaction 
        : this.state$.value.selectedTransaction
    });
  }

  removeTransaction(transactionId: string): void {
    const currentTransactions = this.state$.value.transactions;
    const updatedTransactions = currentTransactions.filter(t => t.id !== transactionId);
    this.updateState({
      transactions: updatedTransactions,
      selectedTransaction: this.state$.value.selectedTransaction?.id === transactionId 
        ? null 
        : this.state$.value.selectedTransaction
    });
  }

  clearError(): void {
    this.updateState({ error: null });
  }

  reset(): void {
    this.state$.next(this.initialState);
  }

  private updateState(partialState: Partial<TransactionState>): void {
    const currentState = this.state$.value;
    this.state$.next({ ...currentState, ...partialState });
  }
}
