//Dummy file for transaction model definitions

export interface Transaction {
  id: string;
  amount: number;
  description: string;
  date: Date;
  type: TransactionType;
  categoryId: string;
  tags: string[];
  notes?: string;
  isRecurring: boolean;
  recurringFrequency?: RecurringFrequency;
  recurringEndDate?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export enum TransactionType {
  EXPENSE = 'EXPENSE',
  INCOME = 'INCOME'
}

export enum RecurringFrequency {
  WEEKLY = 'WEEKLY',
  MONTHLY = 'MONTHLY',
  QUARTERLY = 'QUARTERLY',
  YEARLY = 'YEARLY'
}

export interface CreateTransactionRequest {
  amount: number;
  description: string;
  date: Date;
  type: TransactionType;
  categoryId: string;
  tags: string[];
  notes?: string;
  isRecurring: boolean;
  recurringFrequency?: RecurringFrequency;
  recurringEndDate?: Date;
}

export interface UpdateTransactionRequest extends Partial<CreateTransactionRequest> {
  id: string;
}
