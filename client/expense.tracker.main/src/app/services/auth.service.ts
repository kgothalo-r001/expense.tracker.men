import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { API_CLIENT } from '../app.config';
import { IClient, LoginRequest, RegisterRequest, AuthenticationResult, UserDto } from '../../../auto/autoexpensetrackerclient';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'expense_tracker_token';
  private readonly USER_KEY = 'expense_tracker_user';
  
  private apiClient = inject(API_CLIENT);
  private router = inject(Router);
  
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  private currentUserSubject = new BehaviorSubject<UserDto | null>(this.getCurrentUserFromStorage());
  
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();
  public currentUser$ = this.currentUserSubject.asObservable();
  
  constructor() {
    this.initializeAuth();
  }
  
  private initializeAuth(): void {
    const token = this.getToken();
    if (token) {
      this.loadCurrentUser().subscribe({
        error: () => {
          this.logout();
        }
      });
    }
  }
  
  login(credentials: LoginRequest): Observable<AuthenticationResult> {
    return this.apiClient.login(credentials).pipe(
      tap((result: AuthenticationResult) => {
        if (result.success && result.token) {
          this.setToken(result.token);
          this.setUser(result.user!);
          this.isLoggedInSubject.next(true);
          this.currentUserSubject.next(result.user!);
        }
      }),
      catchError(error => {
        console.error('Login failed:', error);
        return throwError(() => error);
      })
    );
  }
  
  register(userData: RegisterRequest): Observable<AuthenticationResult> {
    return this.apiClient.register(userData).pipe(
      tap((result: AuthenticationResult) => {
        if (result.success && result.token) {
          this.setToken(result.token);
          this.setUser(result.user!);
          this.isLoggedInSubject.next(true);
          this.currentUserSubject.next(result.user!);
        }
      }),
      catchError(error => {
        console.error('Registration failed:', error);
        return throwError(() => error);
      })
    );
  }
  
  logout(): void {
    const token = this.getToken();
    if (token) {
      this.apiClient.logout().subscribe({
        next: () => console.log('Server logout successful'),
        error: (error: any) => console.warn('Server logout failed:', error)
      });
    }
    
    this.clearToken();
    this.clearUser();
    this.isLoggedInSubject.next(false);
    this.currentUserSubject.next(null);
    
    this.router.navigate(['/auth/sign-in']);
  }
  
  loadCurrentUser(): Observable<UserDto> {
    return this.apiClient.getCurrentUser().pipe(
      tap((user: UserDto) => {
        this.setUser(user);
        this.currentUserSubject.next(user);
      }),
      catchError(error => {
        console.error('Failed to load current user:', error);
        return throwError(() => error);
      })
    );
  }
  
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
  
  private setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  
  private clearToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }
  
  private hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }
  
  getCurrentUser(): UserDto | null {
    return this.currentUserSubject.value;
  }
  
  private getCurrentUserFromStorage(): UserDto | null {
    const userStr = localStorage.getItem(this.USER_KEY);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  }
  
  private setUser(user: UserDto): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }
  
  private clearUser(): void {
    localStorage.removeItem(this.USER_KEY);
  }
  
  isAuthenticated(): boolean {
    return this.isLoggedInSubject.value;
  }
  
  getUserDisplayName(): string {
    const user = this.getCurrentUser();
    if (user?.firstName && user?.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    return user?.username || 'User';
  }
}
