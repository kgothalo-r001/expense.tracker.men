import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  IClient,
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest
} from '../../../auto/autoexpensetrackerclient';
import { API_CLIENT } from '../app.config';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  constructor(@Inject(API_CLIENT) private client: IClient) {}

  getAll(): Observable<Category[]> {
    return this.client.getCategories();
  }

  getCategories(): Observable<Category[]> {
    return this.getAll();
  }

  getById(id: string): Observable<Category> {
    return this.client.getCategory(id);
  }

  getCategory(id: string): Observable<Category> {
    return this.getById(id);
  }

  create(category: CreateCategoryRequest): Observable<Category> {
    return this.client.createCategory(category);
  }

  createCategory(category: CreateCategoryRequest): Observable<Category> {
    return this.create(category);
  }

  update(id: string, category: UpdateCategoryRequest): Observable<Category> {
    return this.client.updateCategory(id, category);
  }

  updateCategory(id: string, category: UpdateCategoryRequest): Observable<Category> {
    return this.update(id, category);
  }

  delete(id: string): Observable<void> {
    return this.client.deleteCategory(id);
  }

  deleteCategory(id: string): Observable<void> {
    return this.delete(id);
  }

  initializeDefaultCategories(): Observable<void> {
    return this.client.initializeDefaultCategories();
  }
}
