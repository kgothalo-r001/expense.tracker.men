import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '@expense-tracker/business';
import { LoginRequest } from '../../../../../auto/autoexpensetrackerclient/index';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './sign-in.component.html',
  styleUrl: './sign-in.component.less'
})
export class SignInComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);

  isLoading = signal(false);
  showPassword = signal(false);
  errorMessage = signal<string | null>(null);

  signInForm: FormGroup = this.fb.group({
    usernameOrEmail: ['', [Validators.required]],
    password: ['', [Validators.required]],
    rememberMe: [false]
  });

  onSubmit(): void {
    if (this.signInForm.valid) {
      this.isLoading.set(true);
      this.errorMessage.set(null);

      const { usernameOrEmail, password } = this.signInForm.value;
      const loginRequest: LoginRequest = {
        usernameOrEmail,
        password
      };
      
      this.authService.login(loginRequest).subscribe({
        next: (result) => {
          if (result.success && result.user) {
            this.router.navigate(['/dashboard']);
          } else {
            this.errorMessage.set(result.errorMessage || 'Login failed');
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

  togglePassword(): void {
    this.showPassword.update(show => !show);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.signInForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
}
