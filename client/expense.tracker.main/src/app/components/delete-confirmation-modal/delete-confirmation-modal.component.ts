import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import * as TransactionActions from '../../store/transaction/transaction.actions';
import { AppState } from '../../store/app.state';

@Component({
  selector: 'app-delete-confirmation-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delete-confirmation-modal.component.html',
  styleUrl: './delete-confirmation-modal.component.less'
})
export class DeleteConfirmationModalComponent {
  @Input() isVisible: boolean = false;
  @Input() transactionId: string | null = null;
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() deleted = new EventEmitter<void>();

  isDeleting = false;

  constructor(
    private store: Store<AppState>
  ) {}

  onConfirm(): void {
    if (!this.transactionId) {
      console.error('No transaction ID provided for deletion');
      return;
    }

    this.isDeleting = true;

    this.store.dispatch(TransactionActions.deleteTransaction({ id: this.transactionId }));

    this.isDeleting = false;
    this.deleted.emit();
    this.confirmed.emit();
  }

  onCancel(): void {
    if (!this.isDeleting) {
      this.cancelled.emit();
    }
  }
}
