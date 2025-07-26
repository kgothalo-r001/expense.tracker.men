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
  EXPENSE = 0,
  INCOME = 1
}

export enum RecurringFrequency {
  WEEKLY = 0,
  MONTHLY = 1,
  QUARTERLY = 2,
  YEARLY = 3
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
