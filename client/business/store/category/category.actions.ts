import { createAction, props } from '@ngrx/store';
import { Category } from '../../auto/autobusinessclient';

// Load categories
export const loadCategories = createAction('[Category] Load Categories');

export const loadCategoriesSuccess = createAction(
  '[Category] Load Categories Success',
  props<{ categories: Category[] }>()
);

export const loadCategoriesFailure = createAction(
  '[Category] Load Categories Failure',
  props<{ error: string }>()
);

// Add category
export const addCategory = createAction(
  '[Category] Add Category',
  props<{ category: Partial<Category> }>()
);

export const addCategorySuccess = createAction(
  '[Category] Add Category Success',
  props<{ category: Category }>()
);

export const addCategoryFailure = createAction(
  '[Category] Add Category Failure',
  props<{ error: string }>()
);

// Update category
export const updateCategory = createAction(
  '[Category] Update Category',
  props<{ id: number; category: Partial<Category> }>()
);

export const updateCategorySuccess = createAction(
  '[Category] Update Category Success',
  props<{ category: Category }>()
);

export const updateCategoryFailure = createAction(
  '[Category] Update Category Failure',
  props<{ error: string }>()
);

// Delete category
export const deleteCategory = createAction(
  '[Category] Delete Category',
  props<{ id: number }>()
);

export const deleteCategorySuccess = createAction(
  '[Category] Delete Category Success',
  props<{ id: number }>()
);

export const deleteCategoryFailure = createAction(
  '[Category] Delete Category Failure',
  props<{ error: string }>()
);
