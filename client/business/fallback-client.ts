// Fallback client for when swagger generation fails
/* eslint-disable */

import { Observable, of as _observableOf } from 'rxjs';
import { Injectable, InjectionToken } from '@angular/core';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

// Basic interfaces
export interface MonthlySpending {
  month?: string;
  amount?: number;
}

export interface CategoryTrend {
  category?: string;
  trend?: number;
}

export interface BudgetProjection {
  projected?: number;
  actual?: number;
}

export interface LoginRequest {
  email?: string;
  password?: string;
}

export interface AuthenticationResult {
  token?: string;
  refreshToken?: string;
  success?: boolean;
}

export enum Type {
  Income = "Income",
  Expense = "Expense"
}

export enum Type2 {
  Income = "Income",
  Expense = "Expense"
}

export interface IClient {
  getMonthlySpendingTrends(monthsBack: number | undefined): Observable<MonthlySpending[]>;
  getCategoryTrends(): Observable<CategoryTrend[]>;
  getBudgetProjection(): Observable<BudgetProjection>;
  getMonthlyAverage(type: Type | undefined, monthsBack: number | undefined): Observable<number>;
  getYearlyProjection(type: Type2 | undefined): Observable<number>;
  login(body: LoginRequest | undefined): Observable<AuthenticationResult>;
}

@Injectable({
  providedIn: 'root'
})
export class Client implements IClient {
  private http: any;
  private baseUrl: string;

  constructor() {
    this.baseUrl = '';
  }

  getMonthlySpendingTrends(monthsBack: number | undefined): Observable<MonthlySpending[]> {
    console.warn('Using fallback client - API not available');
    return _observableOf([]);
  }

  getCategoryTrends(): Observable<CategoryTrend[]> {
    console.warn('Using fallback client - API not available');
    return _observableOf([]);
  }

  getBudgetProjection(): Observable<BudgetProjection> {
    console.warn('Using fallback client - API not available');
    return _observableOf({});
  }

  getMonthlyAverage(type: Type | undefined, monthsBack: number | undefined): Observable<number> {
    console.warn('Using fallback client - API not available');
    return _observableOf(0);
  }

  getYearlyProjection(type: Type2 | undefined): Observable<number> {
    console.warn('Using fallback client - API not available');
    return _observableOf(0);
  }

  login(body: LoginRequest | undefined): Observable<AuthenticationResult> {
    console.warn('Using fallback client - API not available');
    return _observableOf({ success: false });
  }
}

export class ApiException extends Error {
  override message: string;
  status: number;
  response: string;
  headers: { [key: string]: any; };
  result: any;

  constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
    super();
    this.message = message;
    this.status = status;
    this.response = response;
    this.headers = headers;
    this.result = result;
  }
}
