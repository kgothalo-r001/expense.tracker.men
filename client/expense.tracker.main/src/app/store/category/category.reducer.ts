import { createReducer, on } from '@ngrx/store';
import { Category } from '../../../../auto/autoexpensetrackerclient';
import * as CategoryActions from './category.actions';

export interface CategoryState {
  categories: Category[];
  loading: boolean;
  error: string | null;
}

export const initialState: CategoryState = {
  categories: [],
  loading: false,
  error: null
};

export const categoryReducer = createReducer(
  initialState,
  
  // Load Categories
  on(CategoryActions.loadCategories, state => ({
    ...state,
    loading: true,
    error: null
  })),
  on(CategoryActions.loadCategoriesSuccess, (state, { categories }) => ({
    ...state,
    categories,
    loading: false,
    error: null
  })),
  on(CategoryActions.loadCategoriesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Add Category
  on(CategoryActions.addCategory, state => ({
    ...state,
    loading: true,
    error: null
  })),
  on(CategoryActions.addCategorySuccess, (state, { category }) => ({
    ...state,
    categories: [...state.categories, category],
    loading: false,
    error: null
  })),
  on(CategoryActions.addCategoryFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Update Category
  on(CategoryActions.updateCategory, state => ({
    ...state,
    loading: true,
    error: null
  })),
  on(CategoryActions.updateCategorySuccess, (state, { category }) => ({
    ...state,
    categories: state.categories.map(c => 
      c.id === category.id ? category : c
    ),
    loading: false,
    error: null
  })),
  on(CategoryActions.updateCategoryFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Delete Category
  on(CategoryActions.deleteCategory, state => ({
    ...state,
    loading: true,
    error: null
  })),
  on(CategoryActions.deleteCategorySuccess, (state, { id }) => ({
    ...state,
    categories: state.categories.filter(c => Number(c.id) !== id),
    loading: false,
    error: null
  })),
  on(CategoryActions.deleteCategoryFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
