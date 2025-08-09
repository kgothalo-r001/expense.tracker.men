import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { TransactionListComponent } from './components/transactions/transaction-list.component';
import { CategoryManagementComponent } from './components/categories/category-management.component';
import { SignInComponent } from './components/auth/sign-in/sign-in.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { AccountComponent } from './components/account/account.component';
import { DefaultRedirectComponent } from './components/default-redirect.component';
import { authGuard, noAuthGuard } from './guards';

export const routes: Routes = [
  { path: '', component: DefaultRedirectComponent },
  { 
    path: 'auth', 
    canActivate: [noAuthGuard],
    children: [
      { path: 'sign-in', component: SignInComponent },
      { path: 'register', component: RegisterComponent },
      { path: '', redirectTo: 'sign-in', pathMatch: 'full' }
    ]
  },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'transactions', component: TransactionListComponent, canActivate: [authGuard] },
  { path: 'categories', component: CategoryManagementComponent, canActivate: [authGuard] },
  { path: 'account', component: AccountComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/auth/sign-in' }
];
