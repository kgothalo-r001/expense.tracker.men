import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { map, switchMap, take } from 'rxjs/operators';
import { of } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  // First check client state, then verify with server if needed
  return authService.isLoggedIn$.pipe(
    take(1),
    switchMap(isLoggedIn => {
      if (isLoggedIn) {
        // User appears to be logged in, allow access
        return of(true);
      } else {
        // User not logged in on client, check server session
        return authService.refreshAuthStatus().pipe(
          map(isAuthenticated => {
            if (isAuthenticated) {
              return true;
            } else {
              router.navigate(['/auth/sign-in']);
              return false;
            }
          })
        );
      }
    })
  );
};
