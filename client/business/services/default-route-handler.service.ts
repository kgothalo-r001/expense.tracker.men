import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class DefaultRouteHandler {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  handleDefaultRoute(): void {
    // Only handle root route - if user has a specific route, don't interfere
    this.authService.isLoggedIn$.pipe(
      switchMap(isLoggedIn => {
        if (isLoggedIn) {
          // User is already authenticated, go to dashboard
          return of(true);
        } else {
          // Check server session
          return this.authService.refreshAuthStatus();
        }
      })
    ).subscribe({
      next: (isAuthenticated) => {
        if (isAuthenticated) {
          this.router.navigate(['/dashboard']);
        } else {
          this.router.navigate(['/auth/sign-in']);
        }
      },
      error: () => {
        this.router.navigate(['/auth/sign-in']);
      }
    });
  }
}
