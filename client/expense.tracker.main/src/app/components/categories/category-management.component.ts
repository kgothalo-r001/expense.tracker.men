import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import { 
  Category, 
  CreateCategoryRequest, 
  CreateCategoryRequestType,
  UpdateCategoryRequest
} from '../../../../auto/autoexpensetrackerclient';
import { CategoryActions, selectAllCategories, selectCategoryLoading, selectCategoryError, AppState } from '../../../../../business';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-management.component.html',
  styleUrl: './category-management.component.less'
})
export class CategoryManagementComponent implements OnInit {
  categories$: Observable<Category[]>;
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;

  // Form state
  isAddingCategory = false;
  editingCategory: Category | null = null;
  newCategoryName = '';
  newCategoryDescription = '';
  newCategoryColor = '#3498db';
  newCategoryType = CreateCategoryRequestType.EXPENSE; // EXPENSE

  categoryTypes = Object.values(CreateCategoryRequestType);
  predefinedColors = [
    '#3498db', '#e74c3c', '#2ecc71', '#f39c12', 
    '#9b59b6', '#1abc9c', '#34495e', '#e67e22'
  ];

  constructor(
    private store: Store<AppState>
  ) {
    this.categories$ = this.store.select(selectAllCategories);
    this.isLoading$ = this.store.select(selectCategoryLoading);
    this.error$ = this.store.select(selectCategoryError);
  }

  ngOnInit(): void {
    this.store.dispatch(CategoryActions.loadCategories());
  }

  onAddCategory(): void {
    this.isAddingCategory = true;
    this.newCategoryName = '';
    this.newCategoryDescription = '';
  }

  onSaveNewCategory(): void {
    if (this.newCategoryName.trim()) {
      const categoryData: CreateCategoryRequest = {
        name: this.newCategoryName.trim(),
        description: this.newCategoryDescription.trim() || undefined,
        color: this.newCategoryColor,
        type: this.newCategoryType
      };

      this.store.dispatch(CategoryActions.addCategory({ category: categoryData as any }));
      this.cancelAddCategory();
    }
  }

  cancelAddCategory(): void {
    this.isAddingCategory = false;
    this.newCategoryName = '';
    this.newCategoryDescription = '';
    this.newCategoryColor = '#3498db';
    this.newCategoryType = CreateCategoryRequestType.EXPENSE; // EXPENSE
  }

  onEditCategory(category: Category): void {
    this.editingCategory = { ...category };
  }

  onSaveEditCategory(): void {
    if (this.editingCategory && this.editingCategory.name?.trim()) {
      const updateRequest: UpdateCategoryRequest = {
        id: this.editingCategory.id!,
        name: this.editingCategory.name.trim(),
        description: this.editingCategory.description?.trim() || undefined,
        color: this.editingCategory.color,
        type: this.editingCategory.type as any
      };

      this.store.dispatch(CategoryActions.updateCategory({ 
        id: Number(this.editingCategory.id!), 
        category: updateRequest as any
      }));
      this.editingCategory = null;
    }
  }

  cancelEditCategory(): void {
    this.editingCategory = null;
  }

  onDeleteCategory(categoryId: string): void {
    if (confirm('Are you sure you want to delete this category? This action cannot be undone.')) {
      this.store.dispatch(CategoryActions.deleteCategory({ id: Number(categoryId) }));
    }
  }

  trackByCategoryId(index: number, category: Category): string {
    return category.id || index.toString();
  }
}
