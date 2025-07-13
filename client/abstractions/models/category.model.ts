//Dummy file for category model definitions

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
  EXPENSE = 'EXPENSE',
  INCOME = 'INCOME',
  BOTH = 'BOTH'
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
