import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Category, CategoryType } from '@abstractions/models';
import { CategoryStateService } from '@business/state';
import { CategoryService } from '@business/services';

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
  newCategoryType = CategoryType.EXPENSE;

  categoryTypes = Object.values(CategoryType);
  predefinedColors = [
    '#3498db', '#e74c3c', '#2ecc71', '#f39c12', 
    '#9b59b6', '#1abc9c', '#34495e', '#e67e22'
  ];

  constructor(
    private categoryStateService: CategoryStateService,
    private categoryService: CategoryService
  ) {
    this.categories$ = this.categoryStateService.categories$;
    this.isLoading$ = this.categoryStateService.isLoading$;
    this.error$ = this.categoryStateService.error$;
  }

  ngOnInit(): void {
    this.categoryStateService.loadCategories();
  }

  onAddCategory(): void {
    this.isAddingCategory = true;
    this.newCategoryName = '';
    this.newCategoryDescription = '';
  }

  onSaveNewCategory(): void {
    if (this.newCategoryName.trim()) {
      const categoryData = {
        name: this.newCategoryName.trim(),
        description: this.newCategoryDescription.trim() || undefined,
        color: this.newCategoryColor,
        type: this.newCategoryType
      };

      this.categoryService.createCategory(categoryData).subscribe({
        next: (category) => {
          this.categoryStateService.addCategory(category);
          this.cancelAddCategory();
        },
        error: (error) => {
          console.error('Failed to create category:', error);
        }
      });
    }
  }

  cancelAddCategory(): void {
    this.isAddingCategory = false;
    this.newCategoryName = '';
    this.newCategoryDescription = '';
    this.newCategoryColor = '#3498db';
    this.newCategoryType = CategoryType.EXPENSE;
  }

  onEditCategory(category: Category): void {
    this.editingCategory = { ...category };
  }

  onSaveEditCategory(): void {
    if (this.editingCategory && this.editingCategory.name.trim()) {
      this.categoryService.updateCategory(this.editingCategory).subscribe({
        next: (category) => {
          this.categoryStateService.updateCategory(category);
          this.editingCategory = null;
        },
        error: (error) => {
          console.error('Failed to update category:', error);
        }
      });
    }
  }

  cancelEditCategory(): void {
    this.editingCategory = null;
  }

  onDeleteCategory(categoryId: string): void {
    if (confirm('Are you sure you want to delete this category? This action cannot be undone.')) {
      this.categoryService.deleteCategory(categoryId).subscribe({
        next: () => {
          this.categoryStateService.removeCategory(categoryId);
        },
        error: (error) => {
          console.error('Failed to delete category:', error);
        }
      });
    }
  }

  trackByCategoryId(index: number, category: Category): string {
    return category.id;
  }
}
