import { TransactionType, RecurringFrequency } from '../auto/autobusinessclient';

/**
 * Validates transaction amount
 */
export function validateAmount(amount: number): string | null {
  if (amount <= 0) {
    return 'Amount must be greater than 0';
  }
  if (amount > 999999.99) {
    return 'Amount cannot exceed R999,999.99';
  }
  return null;
}

/**
 * Validates transaction description
 */
export function validateDescription(description: string): string | null {
  if (!description || description.trim().length === 0) {
    return 'Description is required';
  }
  if (description.length > 255) {
    return 'Description cannot exceed 255 characters';
  }
  return null;
}

/**
 * Validates category selection
 */
export function validateCategory(categoryId: string): string | null {
  if (!categoryId || categoryId.trim().length === 0) {
    return 'Category is required';
  }
  return null;
}

/**
 * Validates transaction date
 */
export function validateDate(date: Date): string | null {
  if (!date) {
    return 'Date is required';
  }
  if (date > new Date()) {
    return 'Date cannot be in the future';
  }
  const minDate = new Date();
  minDate.setFullYear(minDate.getFullYear() - 10);
  if (date < minDate) {
    return 'Date cannot be more than 10 years ago';
  }
  return null;
}

/**
 * Validates recurring transaction settings
 */
export function validateRecurring(
  isRecurring: boolean,
  frequency?: RecurringFrequency,
  endDate?: Date
): string | null {
  if (!isRecurring) {
    return null;
  }
  
  if (!frequency) {
    return 'Recurring frequency is required';
  }
  
  if (endDate && endDate <= new Date()) {
    return 'Recurring end date must be in the future';
  }
  
  return null;
}

/**
 * Validates notes field
 */
export function validateNotes(notes?: string): string | null {
  if (notes && notes.length > 1000) {
    return 'Notes cannot exceed 1000 characters';
  }
  return null;
}

/**
 * Comprehensive transaction validation
 */
export function validateTransaction(transaction: {
  amount: number;
  description: string;
  categoryId: string;
  date: Date;
  isRecurring: boolean;
  recurringFrequency?: RecurringFrequency;
  recurringEndDate?: Date;
  notes?: string;
}): Record<string, string> {
  const errors: Record<string, string> = {};
  
  const amountError = validateAmount(transaction.amount);
  if (amountError) errors['amount'] = amountError;
  
  const descriptionError = validateDescription(transaction.description);
  if (descriptionError) errors['description'] = descriptionError;
  
  const categoryError = validateCategory(transaction.categoryId);
  if (categoryError) errors['categoryId'] = categoryError;
  
  const dateError = validateDate(transaction.date);
  if (dateError) errors['date'] = dateError;
  
  const recurringError = validateRecurring(
    transaction.isRecurring,
    transaction.recurringFrequency,
    transaction.recurringEndDate
  );
  if (recurringError) errors['recurring'] = recurringError;
  
  const notesError = validateNotes(transaction.notes);
  if (notesError) errors['notes'] = notesError;
  
  return errors;
}
