export interface Category {
  id: string;
  name: string;
  description?: string;
  color: string;
  icon?: string;
  type: CategoryType;
  isDefault: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export enum CategoryType {
  EXPENSE = 0,
  INCOME = 1,
  BOTH = 2
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  color: string;
  icon?: string;
  type: CategoryType;
}

export interface UpdateCategoryRequest extends Partial<CreateCategoryRequest> {
  id: string;
}
