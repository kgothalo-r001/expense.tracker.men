import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Transaction, 
  CreateTransactionRequest, 
  UpdateTransactionRequest,
  API_ENDPOINTS 
} from '@expense-tracker/abstractions';

@Injectable({
  providedIn: 'root'
})

//Dummy file for transaction service definitions
export class TransactionService {
  private readonly baseUrl = API_ENDPOINTS.TRANSACTIONS;

  constructor(private http: HttpClient) {}

  getTransactions(params?: { 
    page?: number; 
    pageSize?: number; 
    categoryId?: string; 
    type?: string;
    startDate?: Date;
    endDate?: Date;
    tags?: string[];
  }): Observable<{ transactions: Transaction[]; total: number }> {
    let httpParams = new HttpParams();
    
    if (params?.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    if (params?.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }
    if (params?.type) {
      httpParams = httpParams.set('type', params.type);
    }
    if (params?.startDate) {
      httpParams = httpParams.set('startDate', params.startDate.toISOString());
    }
    if (params?.endDate) {
      httpParams = httpParams.set('endDate', params.endDate.toISOString());
    }
    if (params?.tags?.length) {
      httpParams = httpParams.set('tags', params.tags.join(','));
    }

    return this.http.get<{ transactions: Transaction[]; total: number }>(
      this.baseUrl, 
      { params: httpParams }
    );
  }

  getTransaction(id: string): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.baseUrl}/${id}`);
  }

  createTransaction(transaction: CreateTransactionRequest): Observable<Transaction> {
    return this.http.post<Transaction>(this.baseUrl, transaction);
  }

  updateTransaction(transaction: UpdateTransactionRequest): Observable<Transaction> {
    return this.http.put<Transaction>(`${this.baseUrl}/${transaction.id}`, transaction);
  }

  deleteTransaction(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getRecurringTransactions(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${API_ENDPOINTS.RECURRING}`);
  }
}
