import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter, map, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { ThemeToggleComponent } from './components/theme-toggle/theme-toggle.component';
import { ThemeService } from './services/theme.service';
import { AuthService } from '../../../business';
import { UserDto } from '../../auto/autoexpensetrackerclient';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, ThemeToggleComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.less'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Expense Tracker';
  isAuthPage = false;
  currentUser: UserDto | null = null;
  showUserMenu = false;
  
  private destroy$ = new Subject<void>();
  private themeService = inject(ThemeService);
  private router = inject(Router);
  private authService = inject(AuthService);
  
  ngOnInit(): void {
    // Initialize system theme listener
    this.themeService.initSystemThemeListener();
    
    // Initialize session from server-side cookie
    this.authService.checkSession().subscribe({
      next: (isAuthenticated) => {
      },
      error: (error) => {
      }
    });
    
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        map(event => (event as NavigationEnd).urlAfterRedirects),
        takeUntil(this.destroy$)
      )
      .subscribe(url => {
        this.isAuthPage = url.startsWith('/auth') || url === '/';
        this.showUserMenu = false;
      });
      
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
      });
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  getUserDisplayName(): string {
    return this.authService.getUserDisplayName();
  }
  
  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }
  
  signOut(): void {
    this.authService.logout();
    this.showUserMenu = false;
  }
}
