import { Transaction } from './transaction.model';

// Dummy file for dashboard model definitions
export interface Tag {
  id: string;
  name: string;
  color?: string;
  usageCount: number;
  createdAt: Date;
}

export interface CreateTagRequest {
  name: string;
  color?: string;
}

export interface DashboardSummary {
  totalIncome: number;
  totalExpenses: number;
  netAmount: number;
  transactionCount: number;
  topCategories: CategorySummary[];
  recentTransactions: Transaction[];
}

export interface CategorySummary {
  categoryId: string;
  categoryName: string;
  totalAmount: number;
  transactionCount: number;
  percentage: number;
}

// Re-export transaction models
export * from './transaction.model';
export * from './category.model';
