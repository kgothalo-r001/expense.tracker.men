import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  IClient,
  Transaction,
  CreateTransactionRequest,
  UpdateTransactionRequest
} from '../../../auto/autoexpensetrackerclient';
import { API_CLIENT } from '../app.config';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  constructor(@Inject(API_CLIENT) private client: IClient) {}

  getAll(): Observable<Transaction[]> {
    return this.client.getTransactions(undefined, undefined, undefined);
  }

  getTransactions(): Observable<Transaction[]> {
    return this.getAll();
  }

  getById(id: string): Observable<Transaction> {
    return this.client.getTransaction(id);
  }

  getTransaction(id: string): Observable<Transaction> {
    return this.getById(id);
  }

  create(transaction: CreateTransactionRequest): Observable<Transaction> {
    return this.client.createTransaction(transaction);
  }

  createTransaction(transaction: CreateTransactionRequest): Observable<Transaction> {
    return this.create(transaction);
  }

  update(id: string, transaction: UpdateTransactionRequest): Observable<Transaction> {
    return this.client.updateTransaction(id, transaction);
  }

  updateTransaction(id: string, transaction: UpdateTransactionRequest): Observable<Transaction> {
    return this.update(id, transaction);
  }

  delete(id: string): Observable<void> {
    return this.client.deleteTransaction(id);
  }

  deleteTransaction(id: string): Observable<void> {
    return this.delete(id);
  }
}
