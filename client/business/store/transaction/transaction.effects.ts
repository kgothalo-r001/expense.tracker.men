import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, exhaustMap, take } from 'rxjs/operators';
import { TransactionService } from '../../services/transaction.service';
import { Transaction } from '../../auto/autobusinessclient';
import * as TransactionActions from './transaction.actions';
import { Store } from '@ngrx/store';
import { selectAllTransactions } from './transaction.selectors';

@Injectable()
export class TransactionEffects {
  private actions$ = inject(Actions);
  private transactionService = inject(TransactionService);
  private store = inject(Store);

  loadTransactions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.loadTransactions),
      exhaustMap(() => {
        return this.store.select(selectAllTransactions).pipe(
          take(1),
          exhaustMap((existingTransactions) => {
            if (existingTransactions && existingTransactions.length > 0) {
              return of();
            }

            return this.transactionService.getTransactions(undefined, undefined, undefined).pipe(
              map((transactions: Transaction[]) => 
                TransactionActions.loadTransactionsSuccess({ transactions })
              ),
              catchError((error) => 
                of(TransactionActions.loadTransactionsFailure({ 
                  error: error.message || 'Failed to load transactions' 
                }))
              )
            );
          })
        )
      })
    )
  );

  addTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.addTransaction),
      exhaustMap(({ transaction }) =>
        this.transactionService.createTransaction(transaction as any).pipe(
          map((newTransaction: Transaction) => TransactionActions.addTransactionSuccess({ transaction: newTransaction })),
          catchError((error) => 
            of(TransactionActions.addTransactionFailure({ 
            error: error.message || 'Failed to add transaction' 
          })))
        )
      )
    )
  );

  updateTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.updateTransaction),
      exhaustMap(({ id, transaction }) =>
        this.transactionService.updateTransaction(id, transaction as any).pipe(
          map((updatedTransaction: Transaction) => 
            TransactionActions.updateTransactionSuccess({ transaction: updatedTransaction })),
          catchError((error) => 
            of(TransactionActions.updateTransactionFailure({ 
            error: error.message || 'Failed to update transaction' 
          })))
        )
      )
    )
  );

  deleteTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.deleteTransaction),
      exhaustMap(({ id }) =>
        this.transactionService.deleteTransaction(id).pipe(
          map(() => TransactionActions.deleteTransactionSuccess({ id })),
          catchError((error) => 
            of(TransactionActions.deleteTransactionFailure({ 
            error: error.message || 'Failed to delete transaction' 
          })))
        )
      )
    )
  );
}
