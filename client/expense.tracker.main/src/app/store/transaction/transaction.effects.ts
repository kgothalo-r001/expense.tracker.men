import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { 
  Client, 
  CreateTransactionRequest, 
  UpdateTransactionRequest,
  CreateTransactionRequestType,
  TransactionType
} from '../../../../auto/autoexpensetrackerclient';
import * as TransactionActions from './transaction.actions';

@Injectable()
export class TransactionEffects {
  private actions$ = inject(Actions);
  private client = inject(Client);

  // Helper function to convert TransactionType2 to CreateTransactionRequestType
  private convertToCreateType(type: any): CreateTransactionRequestType {
    return type === 'EXPENSE' ? CreateTransactionRequestType.EXPENSE : CreateTransactionRequestType.INCOME;
  }

  // Helper function to convert TransactionType2 to TransactionType
  private convertToUpdateType(type: any): TransactionType {
    return type === 'EXPENSE' ? TransactionType.EXPENSE : TransactionType.INCOME;
  }

  loadTransactions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.loadTransactions),
      switchMap(() =>
        this.client.getTransactions(undefined, undefined, undefined).pipe(
          map(transactions => TransactionActions.loadTransactionsSuccess({ transactions })),
          catchError(error => of(TransactionActions.loadTransactionsFailure({ 
            error: error.message || 'Failed to load transactions' 
          })))
        )
      )
    )
  );

  addTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.addTransaction),
      switchMap(({ transaction }) => {
        const request: CreateTransactionRequest = {
          amount: transaction.amount!,
          description: transaction.description!,
          date: transaction.date!,
          type: this.convertToCreateType(transaction.type),
          categoryId: transaction.categoryId!,
          tags: transaction.tags,
          notes: transaction.notes,
          isRecurring: transaction.isRecurring,
          recurringFrequency: transaction.recurringFrequency,
          recurringEndDate: transaction.recurringEndDate
        };
        
        return this.client.createTransaction(request).pipe(
          map(transaction => TransactionActions.addTransactionSuccess({ transaction })),
          catchError(error => {
            if (error.status === 201 || error.status === '201') {
              if (error.response) {
                try {
                  const transaction = JSON.parse(error.response);
                  return of(TransactionActions.addTransactionSuccess({ transaction }));
                } catch (parseError) {
                }
              }
            }
            
            let errorMessage = 'Failed to create transaction';
            if (error.message) {
              errorMessage = error.message;
            } else if (error.error?.message) {
              errorMessage = error.error.message;
            }
            
            return of(TransactionActions.addTransactionFailure({ error: errorMessage }));
          })
        );
      })
    )
  );

  updateTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.updateTransaction),
      switchMap(({ id, transaction }) => {
        const request: UpdateTransactionRequest = {
          id: id,
          amount: transaction.amount!,
          description: transaction.description!,
          date: transaction.date!,
          type: this.convertToUpdateType(transaction.type),
          categoryId: transaction.categoryId!,
          tags: transaction.tags,
          notes: transaction.notes,
          isRecurring: transaction.isRecurring,
          recurringFrequency: transaction.recurringFrequency,
          recurringEndDate: transaction.recurringEndDate
        };
        
        return this.client.updateTransaction(id, request).pipe(
          map(transaction => TransactionActions.updateTransactionSuccess({ transaction })),
          catchError(error => of(TransactionActions.updateTransactionFailure({ 
            error: error.message || 'Failed to update transaction' 
          })))
        );
      })
    )
  );

  deleteTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TransactionActions.deleteTransaction),
      switchMap(({ id }) =>
        this.client.deleteTransaction(id).pipe(
          map(() => TransactionActions.deleteTransactionSuccess({ id })),
          catchError(error => of(TransactionActions.deleteTransactionFailure({ 
            error: error.message || 'Failed to delete transaction' 
          })))
        )
      )
    )
  );
}