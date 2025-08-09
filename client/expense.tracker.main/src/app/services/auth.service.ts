import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { API_CLIENT } from '../app.config';
import { IClient, LoginRequest, RegisterRequest, UserDto, AuthenticationResult } from '../../../auto/autoexpensetrackerclient';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiClient = inject(API_CLIENT);
  private router = inject(Router);
  
  private isLoggedInSubject = new BehaviorSubject<boolean>(false);
  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();
  public currentUser$ = this.currentUserSubject.asObservable();
  
  constructor() {
    // Session will be initialized by AppComponent
  }
  
  /**
   * Check if there's a valid session (server-side cookie)
   */
  checkSession(): Observable<boolean> {
    return this.apiClient.checkSession().pipe(
      map((user: UserDto) => {
        if (user) {
          this.isLoggedInSubject.next(true);
          this.currentUserSubject.next(user);
          return true;
        }
        return false;
      }),
      catchError(() => {
        this.isLoggedInSubject.next(false);
        this.currentUserSubject.next(null);
        return of(false);
      })
    );
  }
  
  login(credentials: LoginRequest): Observable<any> {
    return this.apiClient.login(credentials).pipe(
      tap((result: AuthenticationResult) => {
        if (result.success && result.user) {
          this.isLoggedInSubject.next(true);
          this.currentUserSubject.next(result.user);
        }
      }),
      catchError(error => {
        return throwError(() => error);
      })
    );
  }
  
  register(userData: RegisterRequest): Observable<any> {
    return this.apiClient.register(userData).pipe(
      tap((result: AuthenticationResult) => {
        if (result.success && result.user) {
          this.isLoggedInSubject.next(true);
          this.currentUserSubject.next(result.user);
        }
      }),
      catchError(error => {
        return throwError(() => error);
      })
    );
  }
  
  
  logout(): void {
    this.apiClient.logout().subscribe({
      next: () => {
        this.handleLogoutSuccess();
      },
      error: (error: any) => {
        this.handleLogoutSuccess();
      }
    });
  }
  
  private handleLogoutSuccess(): void {
    // Server clears the HTTP-only cookie automatically
    this.isLoggedInSubject.next(false);
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/sign-in']);
  }
  
  getCurrentUser(): UserDto | null {
    return this.currentUserSubject.value;
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
  
  /**
   * Refresh authentication status from server
   * Useful for checking session after page refresh
   */
  refreshAuthStatus(): Observable<boolean> {
    return this.checkSession();
  }
}
