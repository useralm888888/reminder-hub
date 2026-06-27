import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';

import { getErrorMessage } from '../../core/errors/http-error.context';
import { Reminder } from '../../core/models/reminder.model';
import { ReminderService } from '../../core/services/reminder.service';
import {
  buildScheduledAt,
  formatTimeFromDate,
  startOfToday,
} from '../../core/utils/schedule-datetime.util';
import {
  hasFutureDateTimeError,
  scheduledInFutureValidator,
} from '../../core/validators/scheduled-in-future.validator';

export interface ReminderEditDialogData {
  reminder: Reminder;
}

@Component({
  selector: 'app-reminder-edit-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
  ],
  templateUrl: './reminder-edit-dialog.component.html',
  styleUrl: './reminder-edit-dialog.component.scss',
})
export class ReminderEditDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly reminderService = inject(ReminderService);
  private readonly dialogRef = inject(MatDialogRef<ReminderEditDialogComponent, boolean>);
  private readonly data = inject<ReminderEditDialogData>(MAT_DIALOG_DATA);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly hasFutureDateTimeError = hasFutureDateTimeError;
  protected readonly minDate = startOfToday();

  protected readonly form = this.fb.nonNullable.group(
    {
      message: [this.data.reminder.message, [Validators.required, Validators.maxLength(500)]],
      date: [new Date(this.data.reminder.scheduledAt), Validators.required],
      time: [formatTimeFromDate(this.data.reminder.scheduledAt), Validators.required],
      email: [this.data.reminder.email ?? '', Validators.email],
    },
    { validators: scheduledInFutureValidator() },
  );

  protected close(): void {
    this.dialogRef.close(false);
  }

  protected async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { message, date, time, email } = this.form.getRawValue();
    const scheduledAt = buildScheduledAt(date, time);

    this.submitting.set(true);
    this.errorMessage.set(null);

    try {
      await firstValueFrom(
        this.reminderService.update(this.data.reminder.id, {
          message,
          scheduledAt,
          email: email || undefined,
        }),
      );
      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(getErrorMessage(error, 'Could not update reminder.'));
    } finally {
      this.submitting.set(false);
    }
  }
}
