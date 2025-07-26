import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  IClient,
  Transaction,
  CreateTransactionRequest,
  UpdateTransactionRequest
} from '../auto/autobusinessclient';
import { ICLIENT_TOKEN } from '../index';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  constructor(@Inject(ICLIENT_TOKEN) private client: IClient) {}

  getTransactions(categoryId?: string, startDate?: Date, endDate?: Date): Observable<Transaction[]> {
    return this.client.getTransactions(categoryId, startDate, endDate);
  }

  getTransaction(id: string): Observable<Transaction> {
    return this.client.getTransaction(id);
  }

  createTransaction(transaction: CreateTransactionRequest): Observable<Transaction> {
    return this.client.createTransaction(transaction);
  }

  updateTransaction(id: string, transaction: UpdateTransactionRequest): Observable<Transaction> {
    return this.client.updateTransaction(id, transaction);
  }

  deleteTransaction(id: string): Observable<void> {
    return this.client.deleteTransaction(id);
  }

  getRecurringTransactions(): Observable<Transaction[]> {
    return this.client.getRecurringTransactions();
  }

  processRecurringTransactions(): Observable<void> {
    return this.client.processRecurringTransactions();
  }
}
