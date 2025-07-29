import { Transaction, TransactionType, TransactionType2 } from '../auto/autobusinessclient';

export function formatCurrency(amount: number, currency: string = 'R'): string {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(amount);
}

export function formatDate(date: Date, format: 'short' | 'medium' | 'long' = 'medium'): string {
  let options: Intl.DateTimeFormatOptions;
  
  switch (format) {
    case 'short':
      options = { month: 'short', day: 'numeric' };
      break;
    case 'medium':
      options = { month: 'short', day: 'numeric', year: 'numeric' };
      break;
    case 'long':
      options = { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' };
      break;
    default:
      options = { month: 'short', day: 'numeric', year: 'numeric' };
  }

  return new Intl.DateTimeFormat('en-US', options).format(date);
}

export function calculateNetAmount(transactions: Transaction[]): number {
  return transactions.reduce((total, transaction) => {
    if (transaction.type === TransactionType2.INCOME) {
      return total + transaction.amount;
    } else {
      return total - transaction.amount;
    }
  }, 0);
}

export function groupTransactionsByDate(transactions: Transaction[]): { [date: string]: Transaction[] } {
  return transactions.reduce((groups, transaction) => {
    const dateKey = transaction.date ? formatDate(transaction.date, 'medium') : 'Unknown Date';
    if (!groups[dateKey]) {
      groups[dateKey] = [];
    }
    groups[dateKey].push(transaction);
    return groups;
  }, {} as { [date: string]: Transaction[] });
}

export function filterTransactionsByDateRange(
  transactions: Transaction[], 
  startDate: Date, 
  endDate: Date
): Transaction[] {
  return transactions.filter(transaction => {
    if (!transaction.date) return false;
    const transactionDate = new Date(transaction.date);
    return transactionDate >= startDate && transactionDate <= endDate;
  });
}

export function calculateCategoryTotals(transactions: Transaction[]): { [categoryId: string]: number } {
  return transactions.reduce((totals, transaction) => {
    if (!totals[transaction.categoryId]) {
      totals[transaction.categoryId] = 0;
    }
    totals[transaction.categoryId] += transaction.amount;
    return totals;
  }, {} as { [categoryId: string]: number });
}
