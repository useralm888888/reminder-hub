import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';

import { getErrorMessage } from '../../core/errors/http-error.context';
import { Reminder } from '../../core/models/reminder.model';
import { ReminderService } from '../../core/services/reminder.service';
import {
  ReminderEditDialogComponent,
  ReminderEditDialogData,
} from './reminder-edit-dialog.component';

@Component({
  selector: 'app-reminder-list',
  imports: [DatePipe, MatDialogModule, MatIconModule, MatSnackBarModule],
  templateUrl: './reminder-list.component.html',
  styleUrl: './reminder-list.component.scss',
})
export class ReminderListComponent implements OnInit {
  private readonly reminderService = inject(ReminderService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly reminders = this.reminderService.items;
  protected readonly isLoading = this.reminderService.isLoading;
  protected readonly loadError = this.reminderService.loadError;
  protected readonly currentPage = this.reminderService.currentPage;
  protected readonly totalPages = this.reminderService.totalPages;
  protected readonly totalCount = this.reminderService.totalItems;
  protected readonly scheduledCount = this.reminderService.scheduledTotal;
  protected readonly sentCount = this.reminderService.sentTotal;
  protected readonly pageSize = this.reminderService.itemsPerPage;

  protected readonly deletingId = signal<string | null>(null);

  ngOnInit(): void {
    void this.loadPage(1);
  }

  protected loadPage(page: number): void {
    void firstValueFrom(this.reminderService.loadReminders(page)).catch(() => undefined);
  }

  protected goToPreviousPage(): void {
    if (this.currentPage() > 1) {
      this.loadPage(this.currentPage() - 1);
    }
  }

  protected goToNextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.loadPage(this.currentPage() + 1);
    }
  }

  protected canEdit(reminder: Reminder): boolean {
    return reminder.status === 'scheduled';
  }

  protected openEditDialog(reminder: Reminder): void {
    const dialogRef = this.dialog.open<ReminderEditDialogComponent, ReminderEditDialogData, boolean>(
      ReminderEditDialogComponent,
      {
        data: { reminder },
        width: '32rem',
      },
    );

    dialogRef.afterClosed().subscribe((saved) => {
      if (saved) {
        this.snackBar.open('Reminder updated.', 'Close', { duration: 3000 });
        this.loadPage(this.currentPage());
      }
    });
  }

  protected async deleteReminder(reminder: Reminder): Promise<void> {
    const confirmed = globalThis.confirm(`Delete reminder "${reminder.message}"?`);
    if (!confirmed) {
      return;
    }

    this.deletingId.set(reminder.id);

    try {
      await firstValueFrom(this.reminderService.delete(reminder.id));
      this.snackBar.open('Reminder deleted.', 'Close', { duration: 3000 });
      this.loadPage(this.currentPage());
    } catch (error) {
      this.snackBar.open(getErrorMessage(error, 'Could not delete reminder.'), 'Close', {
        duration: 5000,
      });
    } finally {
      this.deletingId.set(null);
    }
  }
}
