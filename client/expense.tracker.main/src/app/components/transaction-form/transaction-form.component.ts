import { Component, OnInit, OnChanges, Input, Output, EventEmitter, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import {
  Transaction, 
  TransactionType, 
  RecurringFrequency, 
  Category,
  CreateTransactionRequest,
  UpdateTransactionRequest
} from '../../../../auto/autoexpensetrackerclient';
import * as CategoryActions from '../../store/';
import * as TransactionActions from '../../store';
import { selectAllCategories } from '../../store';
import { AppState } from '../../store/app.state';

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './transaction-form.component.html',
  styleUrl: './transaction-form.component.less'
})
export class TransactionFormComponent implements OnInit, OnChanges {
  @Input() transaction: Transaction | null = null;
  @Input() isVisible: boolean = false;
  @Input() showModalWrapper: boolean = true; 
  @Output() transactionSaved = new EventEmitter<Transaction>();
  @Output() cancelled = new EventEmitter<void>();

  transactionForm!: FormGroup;
  categories$: Observable<Category[]>;
  transactionTypes = Object.values(TransactionType);
  recurringFrequencies = Object.values(RecurringFrequency);
  
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private store: Store<AppState>
  ) {
    this.categories$ = this.store.select(selectAllCategories);
  }

  ngOnInit(): void {
    this.initializeForm();
    this.store.dispatch(CategoryActions.loadCategories());
    
    if (this.transaction) {
      setTimeout(() => {
        this.populateForm();
      }, 100);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['transaction'] && this.transactionForm && this.transaction) {
      setTimeout(() => {
        this.populateForm();
      }, 100);
    }
  }

  private initializeForm(): void {
    this.transactionForm = this.fb.group({
      description: ['', [Validators.required, Validators.minLength(2)]],
      amount: ['', [Validators.required, Validators.min(0.01)]],
      type: [TransactionType.EXPENSE, Validators.required],
      categoryId: ['', Validators.required],
      date: [new Date().toISOString().split('T')[0], Validators.required],
      notes: [''],
      tags: [''],
      isRecurring: [false],
      recurringFrequency: [RecurringFrequency.MONTHLY],
      recurringEndDate: ['']
    });

    this.transactionForm.get('isRecurring')?.valueChanges.subscribe(isRecurring => {
      const frequencyControl = this.transactionForm.get('recurringFrequency');
      const endDateControl = this.transactionForm.get('recurringEndDate');
      
      if (isRecurring) {
        frequencyControl?.setValidators([Validators.required]);
        endDateControl?.setValidators([Validators.required]);
      } else {
        frequencyControl?.clearValidators();
        endDateControl?.clearValidators();
      }
      
      frequencyControl?.updateValueAndValidity();
      endDateControl?.updateValueAndValidity();
    });
  }

  private populateForm(): void {
    if (this.transaction) {
      this.transactionForm.patchValue({
        description: this.transaction.description,
        amount: this.transaction.amount,
        type: this.transaction.type,
        categoryId: this.transaction.categoryId,
        date: this.transaction.date ? new Date(this.transaction.date).toISOString().split('T')[0] : '',
        notes: this.transaction.notes || '',
        tags: this.transaction.tags?.join(', ') || '',
        isRecurring: this.transaction.isRecurring || false,
        recurringFrequency: this.transaction.recurringFrequency || RecurringFrequency.MONTHLY, // MONTHLY
        recurringEndDate: this.transaction.recurringEndDate 
          ? new Date(this.transaction.recurringEndDate).toISOString().split('T')[0] 
          : ''
      });
      
      // Mark all fields as touched to trigger validation
      this.transactionForm.markAllAsTouched();
      
      // Update validity to ensure proper validation state
      this.transactionForm.updateValueAndValidity();
    }
  }

  onSubmit(): void {
    if (this.transactionForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.errorMessage = '';

      const formValue = this.transactionForm.value;
      const tags = formValue.tags 
        ? formValue.tags.split(',').map((tag: string) => tag.trim()).filter(Boolean)
        : [];

      const transactionData = {
        description: formValue.description,
        amount: parseFloat(formValue.amount),
        type: formValue.type,
        categoryId: formValue.categoryId,
        date: new Date(formValue.date),
        notes: formValue.notes || undefined,
        tags: tags.length > 0 ? tags : undefined,
        isRecurring: formValue.isRecurring,
        recurringFrequency: formValue.isRecurring ? formValue.recurringFrequency : undefined,
        recurringEndDate: formValue.isRecurring && formValue.recurringEndDate 
          ? new Date(formValue.recurringEndDate) 
          : undefined
      };

      if (this.transaction) {
        this.store.dispatch(TransactionActions.updateTransaction({
          id: this.transaction.id!,
          transaction: {
            id: this.transaction.id!,
            ...transactionData,
            recurringFrequency: transactionData.recurringFrequency
          } as any
        }));
      } else {
        this.store.dispatch(TransactionActions.addTransaction({
          transaction: {
            ...transactionData,
            recurringFrequency: transactionData.recurringFrequency
          } as any
        }));
      }

      this.transactionSaved.emit({
        ...transactionData,
        id: this.transaction?.id
      } as Transaction);
      this.resetForm();
      this.isSubmitting = false;
    }
  }

  onCancel(): void {
    this.resetForm();
    this.cancelled.emit();
  }

  private resetForm(): void {
    this.transactionForm.reset({
      type: TransactionType.EXPENSE,
      date: new Date().toISOString().split('T')[0],
      isRecurring: false,
      recurringFrequency: RecurringFrequency.MONTHLY
    });
    this.errorMessage = '';
  }

  get description() { return this.transactionForm.get('description'); }
  get amount() { return this.transactionForm.get('amount'); }
  get type() { return this.transactionForm.get('type'); }
  get categoryId() { return this.transactionForm.get('categoryId'); }
  get date() { return this.transactionForm.get('date'); }
  get isRecurring() { return this.transactionForm.get('isRecurring'); }
  get recurringFrequency() { return this.transactionForm.get('recurringFrequency'); }
  get recurringEndDate() { return this.transactionForm.get('recurringEndDate'); }
}
