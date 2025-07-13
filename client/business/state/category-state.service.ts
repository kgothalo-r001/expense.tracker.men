import { Injectable } from '@angular/core';
import { BehaviorSubject, map, catchError, of, tap } from 'rxjs';
import { Category, LoadingState, LOADING_STATES } from '@expense-tracker/abstractions';
import { CategoryService } from '../services';

interface CategoryState {
  categories: Category[];
  selectedCategory: Category | null;
  loadingState: LoadingState;
  error: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryStateService {
  private readonly initialState: CategoryState = {
    categories: [],
    selectedCategory: null,
    loadingState: LOADING_STATES.IDLE,
    error: null
  };

  private readonly state$ = new BehaviorSubject<CategoryState>(this.initialState);

  // Selectors
  readonly categories$ = this.state$.pipe(map(state => state.categories));
  readonly selectedCategory$ = this.state$.pipe(map(state => state.selectedCategory));
  readonly loadingState$ = this.state$.pipe(map(state => state.loadingState));
  readonly error$ = this.state$.pipe(map(state => state.error));
  readonly isLoading$ = this.loadingState$.pipe(map(state => state === LOADING_STATES.LOADING));

  constructor(private categoryService: CategoryService) {}

  // Actions
  loadCategories(): void {
    this.updateState({ loadingState: LOADING_STATES.LOADING, error: null });
    
    this.categoryService.getCategories().pipe(
      tap(categories => {
        this.updateState({
          categories,
          loadingState: LOADING_STATES.SUCCESS,
          error: null
        });
      }),
      catchError(error => {
        this.updateState({
          loadingState: LOADING_STATES.ERROR,
          error: error.message || 'Failed to load categories'
        });
        return of([]);
      })
    ).subscribe();
  }

  selectCategory(category: Category | null): void {
    this.updateState({ selectedCategory: category });
  }

  addCategory(category: Category): void {
    const currentCategories = this.state$.value.categories;
    this.updateState({
      categories: [...currentCategories, category]
    });
  }

  updateCategory(updatedCategory: Category): void {
    const currentCategories = this.state$.value.categories;
    const updatedCategories = currentCategories.map(c => 
      c.id === updatedCategory.id ? updatedCategory : c
    );
    this.updateState({
      categories: updatedCategories,
      selectedCategory: this.state$.value.selectedCategory?.id === updatedCategory.id 
        ? updatedCategory 
        : this.state$.value.selectedCategory
    });
  }

  removeCategory(categoryId: string): void {
    const currentCategories = this.state$.value.categories;
    const updatedCategories = currentCategories.filter(c => c.id !== categoryId);
    this.updateState({
      categories: updatedCategories,
      selectedCategory: this.state$.value.selectedCategory?.id === categoryId 
        ? null 
        : this.state$.value.selectedCategory
    });
  }

  clearError(): void {
    this.updateState({ error: null });
  }

  reset(): void {
    this.state$.next(this.initialState);
  }

  private updateState(partialState: Partial<CategoryState>): void {
    const currentState = this.state$.value;
    this.state$.next({ ...currentState, ...partialState });
  }
}
