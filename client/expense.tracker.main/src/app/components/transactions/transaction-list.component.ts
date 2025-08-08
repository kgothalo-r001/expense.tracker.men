import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import { Transaction, TransactionType, TransactionType2 } from '../../../../auto/autoexpensetrackerclient';
import * as TransactionActions from '../../store/transaction/transaction.actions';
import { selectAllTransactions, selectTransactionLoading, selectTransactionError } from '../../store/transaction/transaction.selectors';
import { AppState } from '../../store/app.state';
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

  constructor(private store: Store<AppState>) {
    this.transactions$ = this.store.select(selectAllTransactions);
    this.isLoading$ = this.store.select(selectTransactionLoading);
    this.error$ = this.store.select(selectTransactionError);
  }

  ngOnInit(): void {
    this.store.dispatch(TransactionActions.loadTransactions());
  }

  loadTransactions(): void {
    this.store.dispatch(TransactionActions.loadTransactions());
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
    if (this.isEditMode && transaction.id) {
      this.store.dispatch(TransactionActions.updateTransaction({ 
        id: transaction.id, 
        transaction: transaction as any
      }));
    } else {
      this.store.dispatch(TransactionActions.addTransaction({ transaction: transaction as any }));
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
    this.store.dispatch(TransactionActions.loadTransactions());
  }

  trackByTransactionId(index: number, transaction: Transaction): string {
    return transaction.id || index.toString();
  }
}
