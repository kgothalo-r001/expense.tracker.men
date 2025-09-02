import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, take, shareReplay, startWith } from 'rxjs/operators';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../auto/autobusinessclient';
import * as CategoryActions from './category.actions';
import { selectAllCategories } from './category.selectors';

@Injectable()
export class CategoryEffects {
  private actions$ = inject(Actions);
  private categoryService = inject(CategoryService);
  private store = inject(Store);

  private readonly CACHE_DURATION = 5 * 60 * 1000;
  private lastCacheTime = 0;

  private categoriesCache$ = this.categoryService.getCategories().pipe(
    shareReplay(1)
  );

  private refreshCache() {
    this.lastCacheTime = Date.now();
    this.categoriesCache$ = this.categoryService.getCategories().pipe(
      shareReplay(1)
    );
    return this.categoriesCache$;
  }

  private isCacheValid(): boolean {
    return (Date.now() - this.lastCacheTime) < this.CACHE_DURATION;
  }

  loadCategories$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.loadCategories),
      exhaustMap(() => {
        // Check if we already have data
        return this.store.select(selectAllCategories).pipe(
          take(1),
          exhaustMap((existingCategories) => {
            // Only load from API if we don't have any categories yet or cache is expired
            if (existingCategories && existingCategories.length > 0 && this.isCacheValid()) {
              return of(CategoryActions.loadCategoriesSkipped());
            }

            const dataSource$ = this.isCacheValid() ? this.categoriesCache$ : this.refreshCache();
            
            return dataSource$.pipe(
              map((categories: Category[]) => 
                CategoryActions.loadCategoriesSuccess({ categories })
              ),
              catchError((error) => 
                of(CategoryActions.loadCategoriesFailure({ 
                  error: error.message || 'Failed to load categories' 
                }))
              )
            );
          })
        );
      })
    )
  );

  addCategory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.addCategory),
      exhaustMap(({ category }) =>
        this.categoryService.createCategory(category as any).pipe(
          map((newCategory: Category) => {
            this.refreshCache();
            return CategoryActions.addCategorySuccess({ category: newCategory });
          }),
          catchError((error) => 
            of(CategoryActions.addCategoryFailure({ 
              error: error.message || 'Failed to add category' 
            }))
          )
        )
      )
    )
  );

  updateCategory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.updateCategory),
      exhaustMap(({ id, category }) =>
        this.categoryService.updateCategory(id.toString(), category as any).pipe(
          map((updatedCategory: Category) => {
            this.refreshCache();
            return CategoryActions.updateCategorySuccess({ category: updatedCategory });
          }),
          catchError((error) => 
            of(CategoryActions.updateCategoryFailure({ 
              error: error.message || 'Failed to update category' 
            }))
          )
        )
      )
    )
  );

  deleteCategory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.deleteCategory),
      exhaustMap(({ id }) =>
        this.categoryService.deleteCategory(id.toString()).pipe(
          map(() => {
            this.refreshCache();
            return CategoryActions.deleteCategorySuccess({ id });
          }),
          catchError((error) => 
            of(CategoryActions.deleteCategoryFailure({ 
              error: error.message || 'Failed to delete category' 
            }))
          )
        )
      )
    )
  );
}
