import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Observable, tap, catchError, of, debounceTime, switchMap } from 'rxjs';
import { AuthService } from '@expense-tracker/business';
import { RegisterRequest, IClient } from '../../../../../auto/autoexpensetrackerclient';
import { API_CLIENT } from '../../../app.config';

function passwordMatchValidator(control: AbstractControl): { [key: string]: any } | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  
  if (password && confirmPassword && password.value !== confirmPassword.value) {
    return { passwordMismatch: true };
  }
  
  return null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.less']
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);
  private client = inject(API_CLIENT);

  isLoading = signal(false);
  showPassword = signal(false);
  showConfirmPassword = signal(false);
  errorMessage = signal<string | null>(null);
  validationErrors = signal<string[]>([]);
  
  usernameStatus = signal<'checking' | 'available' | 'taken' | null>(null);
  emailStatus = signal<'checking' | 'available' | 'taken' | null>(null);
  usernameSuggestions = signal<string[]>([]);
  
  isCheckingUsername = signal(false);
  isCheckingEmail = signal(false);

  registerForm: FormGroup = this.fb.group({
    firstName: [''],
    lastName: [''],
    username: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],
    acceptTerms: [false, [Validators.requiredTrue]]
  }, { validators: passwordMatchValidator });

  constructor() {
    this.registerForm.get('username')?.valueChanges
      .pipe(debounceTime(500))
      .subscribe(username => {
        if (username && username.length >= 3) {
          this.checkUsernameAvailability(username);
        } else {
          this.usernameStatus.set(null);
          this.usernameSuggestions.set([]);
        }
      });

    this.registerForm.get('email')?.valueChanges
      .pipe(debounceTime(500))
      .subscribe(email => {
        if (email && this.registerForm.get('email')?.valid) {
          this.checkEmailAvailability(email);
        } else {
          this.emailStatus.set(null);
        }
      });
  }

  onSubmit(): void {
    if (this.registerForm.valid && this.canSubmit()) {
      this.isLoading.set(true);
      this.errorMessage.set(null);
      this.validationErrors.set([]);

      const formValue = this.registerForm.value;
      const request: RegisterRequest = {
        username: formValue.username,
        email: formValue.email,
        password: formValue.password,
        confirmPassword: formValue.confirmPassword,
        firstName: formValue.firstName || undefined,
        lastName: formValue.lastName || undefined
      };
      
      this.authService.register(request).subscribe({
        next: (result) => {
          if (result.success && result.user) {
            this.router.navigate(['/dashboard']);
          } else {
            this.errorMessage.set(result.errorMessage || 'Registration failed');
            this.validationErrors.set(result.validationErrors || []);
          }
          this.isLoading.set(false);
        },
        error: (error: any) => {
          let errorMsg = 'An unexpected error occurred. Please try again.';
          if (error.error && error.error.message) {
            errorMsg = error.error.message;
          } else if (error.message) {
            errorMsg = error.message;
          }
          this.errorMessage.set(errorMsg);
          this.isLoading.set(false);
        }
      });
    }
  }

  private checkUsernameAvailability(username: string): void {
    this.isCheckingUsername.set(true);
    this.usernameStatus.set('checking');
    
    this.client.checkUsernameAvailability(username).subscribe({
      next: (available: boolean) => {
        if (available) {
          this.usernameStatus.set('available');
          this.usernameSuggestions.set([]);
        } else {
          this.usernameStatus.set('taken');
          this.getSuggestions(username);
        }
        this.isCheckingUsername.set(false);
      },
      error: () => {
        this.usernameStatus.set(null);
        this.isCheckingUsername.set(false);
      }
    });
  }

  private checkEmailAvailability(email: string): void {
    this.isCheckingEmail.set(true);
    this.emailStatus.set('checking');
    
    this.client.checkEmailAvailability(email).subscribe({
      next: (available: boolean) => {
        this.emailStatus.set(available ? 'available' : 'taken');
        this.isCheckingEmail.set(false);
      },
      error: () => {
        this.emailStatus.set(null);
        this.isCheckingEmail.set(false);
      }
    });
  }

  private getSuggestions(username: string): void {
    this.client.getUsernameSuggestions(username).subscribe({
      next: (suggestions: string[]) => {
        this.usernameSuggestions.set(suggestions);
      },
      error: () => {
        this.usernameSuggestions.set([]);
      }
    });
  }

  selectUsername(username: string): void {
    this.registerForm.patchValue({ username });
    this.usernameSuggestions.set([]);
  }

  togglePassword(): void {
    this.showPassword.update(show => !show);
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword.update(show => !show);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  canSubmit(): boolean {
    return this.usernameStatus() !== 'taken' && 
           this.emailStatus() !== 'taken' &&
           !this.isCheckingUsername() &&
           !this.isCheckingEmail();
  }

  getPasswordStrength(): string {
    const password = this.registerForm.get('password')?.value || '';
    let strength = 0;
    
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;
    
    if (strength <= 2) return 'weak';
    if (strength <= 3) return 'medium';
    return 'strong';
  }

  getPasswordStrengthText(): string {
    const strength = this.getPasswordStrength();
    switch (strength) {
      case 'weak': return 'Weak';
      case 'medium': return 'Medium';
      case 'strong': return 'Strong';
      default: return '';
    }
  }
}
