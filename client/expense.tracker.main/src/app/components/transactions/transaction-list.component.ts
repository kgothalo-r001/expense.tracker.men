import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, combineLatest, map } from 'rxjs';
import { Store } from '@ngrx/store';
import { Transaction, TransactionType, TransactionType2, Category } from '../../../../auto/autoexpensetrackerclient';
import { 
  TransactionActions, 
  CategoryActions,
  selectAllTransactions, 
  selectTransactionLoading, 
  selectTransactionError, 
  selectFilteredTransactions,
  selectAllCategories,
  AppState 
} from '../../../../../business';
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
  categories$: Observable<Category[]>;

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

  constructor(private store: Store<AppState>) {
    this.isLoading$ = this.store.select(selectTransactionLoading);
    this.error$ = this.store.select(selectTransactionError);
    this.categories$ = this.store.select(selectAllCategories);
    
    this.transactions$ = this.store.select(selectAllTransactions);
  }

  private updateFilteredTransactions(): void {
    const filters = {
      type: this.selectedType,
      categoryId: this.selectedCategoryId || undefined,
      startDate: this.startDate ? new Date(this.startDate) : undefined,
      endDate: this.endDate ? new Date(this.endDate) : undefined,
      searchTerm: this.searchTerm.trim() || undefined
    };

    this.transactions$ = this.store.select(selectFilteredTransactions(filters));
  }

  ngOnInit(): void {
    this.store.dispatch(TransactionActions.loadTransactions());
    this.store.dispatch(CategoryActions.loadCategories());
  }

  loadTransactions(): void {
    this.store.dispatch(TransactionActions.loadTransactions());
  }

  onFilterChange(): void {
    this.updateFilteredTransactions();
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
    // Update filtered transactions to show all transactions
    this.updateFilteredTransactions();
  }

  trackByTransactionId(index: number, transaction: Transaction): string {
    return transaction.id || index.toString();
  }
}
