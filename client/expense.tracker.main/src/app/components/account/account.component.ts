import { Component, OnInit, inject, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { UserDto } from '../../../../auto/autoexpensetrackerclient';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './account.component.html',
  styleUrl: './account.component.less'
})
export class AccountComponent implements OnInit, OnDestroy {
  accountForm: FormGroup;
  currentUser: UserDto | null = null;
  isEditing = false;
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  
  private destroy$ = new Subject<void>();
  private formBuilder = inject(FormBuilder);
  private authService = inject(AuthService);
  
  constructor() {
    this.accountForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      username: [{ value: '', disabled: true }],
      email: [{ value: '', disabled: true }]
    });
  }
  
  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (user) {
          this.accountForm.patchValue({
            firstName: user.firstName || '',
            lastName: user.lastName || '',
            username: user.username || '',
            email: user.email || ''
          });
        }
      });
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  toggleEdit(): void {
    this.isEditing = !this.isEditing;
    this.clearMessages();
    
    if (!this.isEditing) {
      if (this.currentUser) {
        this.accountForm.patchValue({
          firstName: this.currentUser.firstName || '',
          lastName: this.currentUser.lastName || ''
        });
      }
    }
  }
  
  saveChanges(): void {
    if (this.accountForm.valid && this.currentUser) {
      this.isLoading = true;
      this.clearMessages();
      
      const updatedData = {
        firstName: this.accountForm.value.firstName,
        lastName: this.accountForm.value.lastName
      };
      
      setTimeout(() => {
        this.isLoading = false;
        this.isEditing = false;
        this.successMessage = 'Account information updated successfully!';
        
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      }, 1000);
      
      // TODO: Implement actual API call when backend endpoint is available
      // this.userService.updateUser(this.currentUser.id, updatedData).subscribe({
      //   next: (updatedUser) => {
      //     this.isLoading = false;
      //     this.isEditing = false;
      //     this.successMessage = 'Account information updated successfully!';
      //     // Update the current user in the auth service
      //     this.authService.updateCurrentUser(updatedUser);
      //   },
      //   error: (error) => {
      //     this.isLoading = false;
      //     this.errorMessage = 'Failed to update account information. Please try again.';
      //     console.error('Update user error:', error);
      //   }
      // });
    }
  }
  
  deleteAccount(): void {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      // TODO: Implement account deletion
      alert('Account deletion is not yet implemented. Please contact support if you need to delete your account.');
    }
  }
  
  signOut(): void {
    this.authService.logout();
  }
  
  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
  
  isFieldInvalid(fieldName: string): boolean {
    const field = this.accountForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
  
  getFieldError(fieldName: string): string {
    const field = this.accountForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} is required`;
      }
      if (field.errors['minlength']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
    }
    return '';
  }
}
