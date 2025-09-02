import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of, forkJoin, merge, Observable } from 'rxjs';
import { map, exhaustMap, catchError, take, mergeMap, shareReplay } from 'rxjs/operators';
import { DashboardService } from '../../services/dashboard.service';
import * as DashboardActions from './dashboard.actions';
import * as CategoryActions from '../category/category.actions';
import * as TransactionActions from '../transaction/transaction.actions';
import { selectDashboardDataExists, selectDashboardLastUpdated } from './dashboard.selectors';
import { 
  DashboardSummary, 
  ExpenseAnalytics, 
  BudgetProjection, 
  MonthlySpending, 
  CategoryTrend 
} from '../../auto/autobusinessclient';

@Injectable()
export class DashboardEffects {
  private actions$ = inject(Actions);
  private dashboardService = inject(DashboardService);
  private store = inject(Store);

  private readonly CACHE_DURATION = 5 * 60 * 1000;

  private dashboardSummary$?: Observable<DashboardSummary>;
  private expenseAnalytics$?: Observable<ExpenseAnalytics>;
  private budgetProjection$?: Observable<BudgetProjection>;
  private monthlySpendingTrends$?: Observable<MonthlySpending[]>;
  private categoryTrends$?: Observable<CategoryTrend[]>;
  private lastCacheTime = 0;

  refreshDashboardOnDataChange$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        
        CategoryActions.addCategorySuccess,
        CategoryActions.updateCategorySuccess,
        CategoryActions.deleteCategorySuccess,
        CategoryActions.loadCategoriesSuccess,
        
        TransactionActions.addTransactionSuccess,
        TransactionActions.updateTransactionSuccess,
        TransactionActions.deleteTransactionSuccess,
        TransactionActions.loadTransactionsSuccess
      ),
      map(() => {
        this.refreshCache();
        return DashboardActions.refreshDashboardData();
      })
    )
  );

  loadAllDashboardData$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadAllDashboardData),
      exhaustMap(() => {
        return this.store.select(selectDashboardDataExists).pipe(
          take(1),
          mergeMap((dataExists) => {
            if (dataExists && this.isCacheValid()) {
              return of(DashboardActions.loadAllDashboardDataSkipped());
            }

            return this.loadAllData();
          })
        );
      })
    )
  );

  refreshDashboardData$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.refreshDashboardData),
      mergeMap(() => this.loadAllData())
    )
  );

  loadDashboardSummary$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadDashboardSummary),
      exhaustMap(() => {
        if (!this.dashboardSummary$ || !this.isCacheValid()) {
          this.dashboardSummary$ = this.dashboardService.getDashboardSummary().pipe(
            shareReplay(1)
          );
          this.updateCacheTime();
        }

        return this.dashboardSummary$.pipe(
          map((summary) => DashboardActions.loadDashboardSummarySuccess({ summary })),
          catchError((error) =>
            of(DashboardActions.loadDashboardSummaryFailure({ error: error.message || 'Failed to load summary' }))
          )
        );
      })
    )
  );

  loadExpenseAnalytics$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadExpenseAnalytics),
      exhaustMap(() => {
        if (!this.expenseAnalytics$ || !this.isCacheValid()) {
          this.expenseAnalytics$ = this.dashboardService.getExpenseAnalytics().pipe(
            shareReplay(1)
          );
          this.updateCacheTime();
        }

        return this.expenseAnalytics$.pipe(
          map((analytics) => DashboardActions.loadExpenseAnalyticsSuccess({ analytics })),
          catchError((error) =>
            of(DashboardActions.loadExpenseAnalyticsFailure({ error: error.message || 'Failed to load analytics' }))
          )
        );
      })
    )
  );

  loadBudgetProjection$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadBudgetProjection),
      exhaustMap(() => {
        if (!this.budgetProjection$ || !this.isCacheValid()) {
          this.budgetProjection$ = this.dashboardService.getBudgetProjection().pipe(
            shareReplay(1)
          );
          this.updateCacheTime();
        }

        return this.budgetProjection$.pipe(
          map((projection) => DashboardActions.loadBudgetProjectionSuccess({ projection })),
          catchError((error) =>
            of(DashboardActions.loadBudgetProjectionFailure({ error: error.message || 'Failed to load projection' }))
          )
        );
      })
    )
  );

  loadMonthlySpendingTrends$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadMonthlySpendingTrends),
      exhaustMap(() => {
        if (!this.monthlySpendingTrends$ || !this.isCacheValid()) {
          this.monthlySpendingTrends$ = this.dashboardService.getMonthlySpendingTrends().pipe(
            shareReplay(1)
          );
          this.updateCacheTime();
        }

        return this.monthlySpendingTrends$.pipe(
          map((trends) => DashboardActions.loadMonthlySpendingTrendsSuccess({ trends })),
          catchError((error) =>
            of(DashboardActions.loadMonthlySpendingTrendsFailure({ error: error.message || 'Failed to load trends' }))
          )
        );
      })
    )
  );

  loadCategoryTrends$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadCategoryTrends),
      exhaustMap(() => {
        if (!this.categoryTrends$ || !this.isCacheValid()) {
          this.categoryTrends$ = this.dashboardService.getCategoryTrends().pipe(
            shareReplay(1)
          );
          this.updateCacheTime();
        }

        return this.categoryTrends$.pipe(
          map((trends) => DashboardActions.loadCategoryTrendsSuccess({ trends })),
          catchError((error) =>
            of(DashboardActions.loadCategoryTrendsFailure({ error: error.message || 'Failed to load category trends' }))
          )
        );
      })
    )
  );

  private loadAllData() {
    this.refreshCache();
    
    this.dashboardSummary$ = this.dashboardService.getDashboardSummary().pipe(shareReplay(1));
    this.expenseAnalytics$ = this.dashboardService.getExpenseAnalytics().pipe(shareReplay(1));
    this.budgetProjection$ = this.dashboardService.getBudgetProjection().pipe(shareReplay(1));
    this.monthlySpendingTrends$ = this.dashboardService.getMonthlySpendingTrends().pipe(shareReplay(1));
    this.categoryTrends$ = this.dashboardService.getCategoryTrends().pipe(shareReplay(1));
    
    this.updateCacheTime();

    return merge(
      this.dashboardSummary$.pipe(
        map((summary) => DashboardActions.loadDashboardSummarySuccess({ summary })),
        catchError((error) => of(DashboardActions.loadDashboardSummaryFailure({ error: error.message })))
      ),
      this.expenseAnalytics$.pipe(
        map((analytics) => DashboardActions.loadExpenseAnalyticsSuccess({ analytics })),
        catchError((error) => of(DashboardActions.loadExpenseAnalyticsFailure({ error: error.message })))
      ),
      this.budgetProjection$.pipe(
        map((projection) => DashboardActions.loadBudgetProjectionSuccess({ projection })),
        catchError((error) => of(DashboardActions.loadBudgetProjectionFailure({ error: error.message })))
      ),
      this.monthlySpendingTrends$.pipe(
        map((trends) => DashboardActions.loadMonthlySpendingTrendsSuccess({ trends })),
        catchError((error) => of(DashboardActions.loadMonthlySpendingTrendsFailure({ error: error.message })))
      ),
      this.categoryTrends$.pipe(
        map((trends) => DashboardActions.loadCategoryTrendsSuccess({ trends })),
        catchError((error) => of(DashboardActions.loadCategoryTrendsFailure({ error: error.message })))
      )
    );
  }

  private refreshCache(): void {
    this.dashboardSummary$ = undefined;
    this.expenseAnalytics$ = undefined;
    this.budgetProjection$ = undefined;
    this.monthlySpendingTrends$ = undefined;
    this.categoryTrends$ = undefined;
    this.lastCacheTime = 0;
  }

  private isCacheValid(): boolean {
    const now = new Date().getTime();
    return (now - this.lastCacheTime) < this.CACHE_DURATION;
  }

  private updateCacheTime(): void {
    this.lastCacheTime = new Date().getTime();
  }
}
