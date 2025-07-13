import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Transaction, TransactionType } from '@abstractions/models';
import { TransactionStateService } from '@business/state';
import { TransactionService } from '@business/services';

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transaction-list.component.html',
  styleUrl: './transaction-list.component.less'
})
export class TransactionListComponent implements OnInit {
  transactions$: Observable<Transaction[]>;
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;

  // Filter properties
  selectedType: TransactionType | 'ALL' = 'ALL';
  selectedCategoryId: string = '';
  startDate: string = '';
  endDate: string = '';
  searchTerm: string = '';

  transactionTypes = Object.values(TransactionType);

  constructor(
    private transactionStateService: TransactionStateService,
    private transactionService: TransactionService
  ) {
    this.transactions$ = this.transactionStateService.transactions$;
    this.isLoading$ = this.transactionStateService.isLoading$;
    this.error$ = this.transactionStateService.error$;
  }

  ngOnInit(): void {
    this.transactionStateService.loadTransactions();
  }

  loadTransactions(): void {
    const filters: any = {};
    
    if (this.selectedType !== 'ALL') {
      filters.type = this.selectedType;
    }
    if (this.selectedCategoryId) {
      filters.categoryId = this.selectedCategoryId;
    }
    if (this.startDate) {
      filters.startDate = new Date(this.startDate);
    }
    if (this.endDate) {
      filters.endDate = new Date(this.endDate);
    }

    this.transactionStateService.setFilters(filters);
  }

  onFilterChange(): void {
    this.loadTransactions();
  }

  onDeleteTransaction(id: string): void {
    if (confirm('Are you sure you want to delete this transaction?')) {
      this.transactionStateService.removeTransaction(id);
    }
  }

  onEditTransaction(transaction: Transaction): void {
    // Navigate to edit form or open modal
    console.log('Edit transaction:', transaction);
  }

  clearFilters(): void {
    this.selectedType = 'ALL';
    this.selectedCategoryId = '';
    this.startDate = '';
    this.endDate = '';
    this.searchTerm = '';
    this.transactionStateService.setFilters({});
  }

  trackByTransactionId(index: number, transaction: Transaction): string {
    return transaction.id;
  }
}
