import { Component, inject, signal } from '@angular/core';
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

export interface ReminderEditDialogData {
  reminder: Reminder;
}

@Component({
  selector: 'app-reminder-edit-dialog',
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

  protected readonly form = this.fb.nonNullable.group({
    message: [this.data.reminder.message, [Validators.required, Validators.maxLength(500)]],
    date: [new Date(this.data.reminder.scheduledAt), Validators.required],
    time: [this.formatTime(this.data.reminder.scheduledAt), Validators.required],
    email: [this.data.reminder.email ?? '', Validators.email],
  });

  protected close(): void {
    this.dialogRef.close(false);
  }

  protected async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { message, date, time, email } = this.form.getRawValue();
    const [hours, minutes] = time.split(':').map(Number);
    const scheduledAt = new Date(date);
    scheduledAt.setHours(hours, minutes, 0, 0);

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

  private formatTime(date: Date): string {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }
}
