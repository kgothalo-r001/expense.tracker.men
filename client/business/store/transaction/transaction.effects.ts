import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { 
  Client, 
  CreateTransactionRequest, 
  UpdateTransactionRequest
} from '../../auto/autobusinessclient';
import * as TransactionActions from './transaction.actions';

@Injectable()
export class TransactionEffects {
  private actions$ = inject(Actions);
  private client = inject(Client);

  loadTransactions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.loadTransactions),
      switchMap(() =>
        this.client.getTransactions(undefined, undefined, undefined).pipe(
          map((transactions) => TransactionActions.loadTransactionsSuccess({ transactions })),
          catchError((error) => of(TransactionActions.loadTransactionsFailure({ 
            error: error.message || 'Failed to load transactions' 
          })))
        )
      )
    )
  );

  addTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.addTransaction),
      switchMap(({ transaction }) =>
        this.client.createTransaction(transaction as any).pipe(
          map((newTransaction) => TransactionActions.addTransactionSuccess({ transaction: newTransaction })),
          catchError((error) => of(TransactionActions.addTransactionFailure({ 
            error: error.message || 'Failed to add transaction' 
          })))
        )
      )
    )
  );

  updateTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.updateTransaction),
      switchMap(({ id, transaction }) =>
        this.client.updateTransaction(id, transaction as any).pipe(
          map((updatedTransaction) => TransactionActions.updateTransactionSuccess({ transaction: updatedTransaction })),
          catchError((error) => of(TransactionActions.updateTransactionFailure({ 
            error: error.message || 'Failed to update transaction' 
          })))
        )
      )
    )
  );

  deleteTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.deleteTransaction),
      switchMap(({ id }) =>
        this.client.deleteTransaction(id).pipe(
          map(() => TransactionActions.deleteTransactionSuccess({ id })),
          catchError((error) => of(TransactionActions.deleteTransactionFailure({ 
            error: error.message || 'Failed to delete transaction' 
          })))
        )
      )
    )
  );
}
