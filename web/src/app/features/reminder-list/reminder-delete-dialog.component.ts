import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface ReminderDeleteDialogData {
  message: string;
}

@Component({
  selector: 'app-reminder-delete-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatDialogModule],
  templateUrl: './reminder-delete-dialog.component.html',
  styleUrl: './reminder-delete-dialog.component.scss',
})
export class ReminderDeleteDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ReminderDeleteDialogComponent, boolean>);
  protected readonly data = inject<ReminderDeleteDialogData>(MAT_DIALOG_DATA);

  protected cancel(): void {
    this.dialogRef.close(false);
  }

  protected confirm(): void {
    this.dialogRef.close(true);
  }
}
