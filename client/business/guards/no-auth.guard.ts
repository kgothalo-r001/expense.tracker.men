import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const noAuthGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  return authService.isLoggedIn$.pipe(
    take(1),
    map(isLoggedIn => {
      if (!isLoggedIn) {
        return true;
      } else {
        router.navigate(['/dashboard']);
        return false;
      }
    })
  );
};
