import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TransactionService } from '../../../../../business/services/transaction.service';
import { TransactionStateService } from '../../../../../business/state/transaction-state.service';

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
    private transactionService: TransactionService,
    private transactionStateService: TransactionStateService
  ) {}

  onConfirm(): void {
    if (!this.transactionId) {
      console.error('No transaction ID provided for deletion');
      return;
    }

    this.isDeleting = true;

    this.transactionService.deleteTransaction(this.transactionId).subscribe({
      next: () => {
        // Remove from state management
        this.transactionStateService.removeTransaction(this.transactionId!);
        
        this.isDeleting = false;
        this.deleted.emit();
        this.confirmed.emit();
      },
      error: (error) => {
        console.error('Error deleting transaction:', error);
        this.isDeleting = false;
        // Optionally emit an error event or show an error message
      }
    });
  }

  onCancel(): void {
    if (!this.isDeleting) {
      this.cancelled.emit();
    }
  }
}
