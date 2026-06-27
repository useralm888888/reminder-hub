import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { getErrorMessage } from '../../core/errors/http-error.context';
import { ReminderService } from '../../core/services/reminder.service';
import { buildScheduledAt, isScheduleDateAllowed } from '../../core/utils/schedule-datetime.util';
import {
  SCHEDULED_IN_FUTURE_MESSAGE,
  scheduledInFutureValidator,
  shouldShowFutureDateTimeError,
} from '../../core/validators/scheduled-in-future.validator';

@Component({
  selector: 'app-scheduling',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatNativeDateModule,
    MatSnackBarModule,
  ],
  templateUrl: './scheduling.component.html',
  styleUrl: './scheduling.component.scss',
})
export class SchedulingComponent {
  private readonly fb = inject(FormBuilder);
  private readonly reminderService = inject(ReminderService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  protected readonly submitting = signal(false);
  protected readonly submitAttempted = signal(false);
  protected readonly scheduledInFutureMessage = SCHEDULED_IN_FUTURE_MESSAGE;
  protected readonly shouldShowFutureDateTimeError = shouldShowFutureDateTimeError;
  protected readonly scheduleDateFilter = isScheduleDateAllowed;

  protected readonly form = this.fb.nonNullable.group(
    {
      message: ['', [Validators.required, Validators.maxLength(500)]],
      date: [new Date(), Validators.required],
      time: ['12:00', Validators.required],
      email: ['', Validators.email],
    },
    { validators: scheduledInFutureValidator() },
  );

  protected async submit(): Promise<void> {
    this.form.updateValueAndValidity();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.submitAttempted.set(true);
      this.cdr.markForCheck();

      if (this.form.hasError('futureDateTime')) {
        this.snackBar.open(SCHEDULED_IN_FUTURE_MESSAGE, 'Close', { duration: 5000 });
      }

      return;
    }

    this.submitAttempted.set(false);

    const { message, date, time, email } = this.form.getRawValue();
    const scheduledAt = buildScheduledAt(date, time);

    this.submitting.set(true);

    try {
      await firstValueFrom(
        this.reminderService.create({
          message,
          scheduledAt,
          email: email || undefined,
        }),
      );

      this.snackBar.open('Reminder created.', 'Close', { duration: 3000 });
      this.form.reset({
        message: '',
        date: new Date(),
        time: '12:00',
        email: '',
      });
      void this.router.navigate(['/list']);
    } catch (error) {
      this.snackBar.open(
        getErrorMessage(error, 'Could not create reminder. Please try again.'),
        'Close',
        { duration: 5000 },
      );
    } finally {
      this.submitting.set(false);
    }
  }
}
