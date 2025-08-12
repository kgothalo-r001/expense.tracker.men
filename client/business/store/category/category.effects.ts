import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, take } from 'rxjs/operators';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../auto/autobusinessclient';
import * as CategoryActions from './category.actions';
import { selectAllCategories } from './category.selectors';

@Injectable()
export class CategoryEffects {
  private actions$ = inject(Actions);
  private categoryService = inject(CategoryService);
  private store = inject(Store);

  loadCategories$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.loadCategories),
      exhaustMap(() => {
        // Check if we already have data
        return this.store.select(selectAllCategories).pipe(
          take(1),
          exhaustMap((existingCategories) => {
            // Only load from API if we don't have any categories yet
            if (existingCategories && existingCategories.length > 0) {
              return of(); // Return empty observable to prevent API call
            }

            return this.categoryService.getCategories().pipe(
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
          map((newCategory: Category) => 
            CategoryActions.addCategorySuccess({ category: newCategory })
          ),
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
          map((updatedCategory: Category) => 
            CategoryActions.updateCategorySuccess({ category: updatedCategory })
          ),
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
          map(() => CategoryActions.deleteCategorySuccess({ id })),
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
