import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AppCategoryState } from './category.reducer';

export const selectCategoryState = createFeatureSelector<AppCategoryState>('categories');

export const selectAllCategories = createSelector(
  selectCategoryState,
  (state: AppCategoryState) => state.categories
);

export const selectCategoryLoading = createSelector(
  selectCategoryState,
  (state: AppCategoryState) => state.loading
);

export const selectCategoryError = createSelector(
  selectCategoryState,
  (state: AppCategoryState) => state.error
);

export const selectCategoryById = (id: string) => createSelector(
  selectAllCategories,
  (categories) => categories.find((category: any) => category.id === id)
);
