import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  IClient,
  Category,
  Transaction,
  Tag,
  DashboardSummary,
  ExpenseAnalytics,
  BudgetProjection,
  MonthlySpending,
  CategoryTrend,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  CreateTransactionRequest,
  UpdateTransactionRequest,
  CreateTagRequest,
  TransactionType,
  Type,
  Type2
} from '@business/auto';
import { API_CLIENT } from '../app.config';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  constructor(@Inject(API_CLIENT) private client: IClient) {}

  // Categories
  getCategories(): Observable<Category[]> {
    return this.client.getCategories();
  }

  getCategory(id: string): Observable<Category> {
    return this.client.getCategory(id);
  }

  createCategory(category: CreateCategoryRequest): Observable<Category> {
    return this.client.createCategory(category);
  }

  updateCategory(id: string, category: UpdateCategoryRequest): Observable<Category> {
    return this.client.updateCategory(id, category);
  }

  deleteCategory(id: string): Observable<void> {
    return this.client.deleteCategory(id);
  }

  initializeDefaultCategories(): Observable<void> {
    return this.client.initializeDefaultCategories();
  }

  // Transactions
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

  // Dashboard
  getDashboardSummary(startDate?: Date, endDate?: Date): Observable<DashboardSummary> {
    return this.client.getDashboardSummary(startDate, endDate);
  }

  getExpenseAnalytics(monthsBack?: number): Observable<ExpenseAnalytics> {
    return this.client.getExpenseAnalytics(monthsBack);
  }

  getBudgetProjection(): Observable<BudgetProjection> {
    return this.client.getBudgetProjection2();
  }

  // Tags
  getTags(): Observable<Tag[]> {
    return this.client.getTags();
  }

  getTag(id: string): Observable<Tag> {
    return this.client.getTag(id);
  }

  createTag(tag: CreateTagRequest): Observable<Tag> {
    return this.client.createTag(tag);
  }

  deleteTag(id: string): Observable<void> {
    return this.client.deleteTag(id);
  }

  getPopularTags(limit?: number): Observable<Tag[]> {
    return this.client.getPopularTags(limit);
  }

  // Analytics
  getMonthlySpendingTrends(monthsBack?: number): Observable<MonthlySpending[]> {
    return this.client.getMonthlySpendingTrends(monthsBack);
  }

  getCategoryTrends(): Observable<CategoryTrend[]> {
    return this.client.getCategoryTrends();
  }

  getBudgetProjectionAnalytics(): Observable<BudgetProjection> {
    return this.client.getBudgetProjection();
  }

  getMonthlyAverage(type?: Type, monthsBack?: number): Observable<number> {
    return this.client.getMonthlyAverage(type, monthsBack);
  }

  getYearlyProjection(type?: Type2): Observable<number> {
    return this.client.getYearlyProjection(type);
  }
}
