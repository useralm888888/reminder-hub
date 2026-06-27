import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { getErrorMessage, isHttpErrorContext } from '../../../core/errors/http-error.context';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly showPassword = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  protected togglePasswordVisibility(): void {
    this.showPassword.update((visible) => !visible);
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    try {
      const { username, password } = this.form.getRawValue();
      await firstValueFrom(this.authService.login({ username, password }));
      await this.router.navigate(['/scheduling']);
    } catch (error) {
      if (isHttpErrorContext(error) && error.response.status === 401) {
        this.errorMessage.set('Invalid username or password.');
      } else {
        this.errorMessage.set(getErrorMessage(error, 'Could not sign in. Please try again.'));
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
