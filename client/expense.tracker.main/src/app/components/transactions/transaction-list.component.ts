import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Transaction, TransactionType } from '@business/auto';
import { TransactionStateService } from '@business/state';
import { TransactionService } from '@business/services';
import { TransactionModalComponent } from './transaction-modal.component';
import { DeleteConfirmationModalComponent } from '../delete-confirmation-modal';

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TransactionModalComponent, DeleteConfirmationModalComponent],
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

  showTransactionModal = false;
  selectedTransaction: Transaction | null = null;
  isEditMode = false;

  showDeleteModal = false;
  transactionToDelete: string | null = null;

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
    this.transactionToDelete = id;
    this.showDeleteModal = true;
  }

  onDeleteConfirmed(): void {
    this.showDeleteModal = false;
    this.transactionToDelete = null;
  }

  onDeleteCancelled(): void {
    this.showDeleteModal = false;
    this.transactionToDelete = null;
  }

  onTransactionDeleted(): void {
    this.showDeleteModal = false;
    this.transactionToDelete = null;
  }

  onAddTransaction(): void {
    this.selectedTransaction = null;
    this.isEditMode = false;
    this.showTransactionModal = true;
  }

  onEditTransaction(transaction: Transaction): void {
    this.selectedTransaction = transaction;
    this.isEditMode = true;
    this.showTransactionModal = true;
  }

  onTransactionSaved(transaction: Transaction): void {
    if (this.isEditMode) {
      this.transactionStateService.updateTransaction(transaction);
    } else {
      this.transactionStateService.addTransaction(transaction);
    }
    this.showTransactionModal = false;
    this.selectedTransaction = null;
    this.isEditMode = false;
  }

  onTransactionModalClosed(): void {
    this.showTransactionModal = false;
    this.selectedTransaction = null;
    this.isEditMode = false;
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
    return transaction.id || index.toString();
  }
}
