import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  IClient,
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest
} from '../auto/autobusinessclient';
import { ICLIENT_TOKEN } from '../index';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  constructor(@Inject(ICLIENT_TOKEN) private client: IClient) {}

  getCategories(): Observable<Category[]> {
    return this.client.getCategories();
  }

  getCategory(id: string): Observable<Category> {
    return this.client.getCategory(id);
  }

  createCategory(category: CreateCategoryRequest): Observable<Category> {
    return this.client.createCategory(category);
  }

  updateCategory(id: string, category: UpdateCategoryRequest): Observable<Category> {
    return this.client.updateCategory(id, category);
  }

  deleteCategory(id: string): Observable<void> {
    return this.client.deleteCategory(id);
  }

  initializeDefaultCategories(): Observable<void> {
    return this.client.initializeDefaultCategories();
  }
}
