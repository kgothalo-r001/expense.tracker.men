import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Transaction } from '../../../../auto/autoexpensetrackerclient';
import { TransactionFormComponent } from '../transaction-form/transaction-form.component';

@Component({
  selector: 'app-transaction-modal',
  standalone: true,
  imports: [CommonModule, TransactionFormComponent],
  template: `
    <div class="modal-overlay" *ngIf="isVisible" (click)="onClose()">
      <div class="modal-container" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ transaction ? 'Edit' : 'Add' }} Transaction</h2>
          <button type="button" class="btn-close" (click)="onClose()" aria-label="Close">
            <span aria-hidden="true">&times;</span>
          </button>
        </div>
        
        <div class="modal-body">
          <app-transaction-form
            [transaction]="transaction"
            [isVisible]="false"
            [showModalWrapper]="false"
            (transactionSaved)="onTransactionSaved($event)"
            (cancelled)="onClose()">
          </app-transaction-form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }

    .modal-container {
      background: var(--background-color, white);
      border-radius: 8px;
      min-width: 600px;
      max-width: 90vw;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 1.5rem;
      border-bottom: 1px solid var(--border-color, #e0e0e0);
    }

    .modal-header h2 {
      margin: 0;
      color: var(--primary-text-color, #333);
      font-size: 1.25rem;
      font-weight: 600;
    }

    .btn-close {
      background: none;
      border: none;
      font-size: 1.5rem;
      cursor: pointer;
      color: var(--secondary-text-color, #666);
      padding: 0;
      width: 30px;
      height: 30px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 4px;
      transition: background-color 0.2s ease;
    }

    .btn-close:hover {
      background-color: var(--hover-background-color, #f5f5f5);
      color: var(--primary-text-color, #333);
    }

    .modal-body {
      padding: 0;
    }

    /* Hide the form's built-in modal styling since we're providing our own */
    :host ::ng-deep .transaction-form-overlay {
      position: static;
      background: none;
      display: block;
    }

    :host ::ng-deep .transaction-form-modal {
      background: none;
      box-shadow: none;
      border-radius: 0;
      width: 100%;
      max-width: none;
      margin: 0;
    }

    :host ::ng-deep .form-header {
      display: none;
    }
  `]
})
export class TransactionModalComponent implements OnChanges {
  @Input() transaction: Transaction | null = null;
  @Input() isVisible: boolean = false;
  @Output() transactionSaved = new EventEmitter<Transaction>();
  @Output() closed = new EventEmitter<void>();

  ngOnChanges(changes: SimpleChanges): void {
  }

  onTransactionSaved(transaction: Transaction): void {
    this.transactionSaved.emit(transaction);
    this.onClose();
  }

  onClose(): void {
    this.closed.emit();
  }
}
