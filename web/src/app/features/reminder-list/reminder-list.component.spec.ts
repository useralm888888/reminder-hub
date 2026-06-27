import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';

import { ReminderHubService } from '../../core/services/reminder-hub.service';
import { ReminderService } from '../../core/services/reminder.service';
import { ReminderDeleteDialogComponent } from './reminder-delete-dialog.component';
import { ReminderListComponent } from './reminder-list.component';

function createReminderServiceMock(deleteSpy = vi.fn()) {
  return {
    items: signal([]),
    isLoading: signal(false),
    loadError: signal(null),
    currentPage: signal(1),
    totalPages: signal(1),
    totalItems: signal(0),
    scheduledTotal: signal(0),
    sentTotal: signal(0),
    itemsPerPage: signal(50),
    loadReminders: vi.fn().mockReturnValue(of([])),
    delete: deleteSpy,
  };
}

function createHubServiceMock() {
  return {
    connectionFailed: signal(false),
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
  };
}

describe('ReminderListComponent', () => {
  it('opens a delete dialog before removing a reminder', async () => {
    const deleteSpy = vi.fn().mockReturnValue(of(undefined));
    const dialogOpen = vi.fn().mockReturnValue({
      afterClosed: () => of(true),
    });

    await TestBed.configureTestingModule({
      imports: [ReminderListComponent],
      providers: [
        { provide: ReminderService, useValue: createReminderServiceMock(deleteSpy) },
        { provide: ReminderHubService, useValue: createHubServiceMock() },
        { provide: MatDialog, useValue: { open: dialogOpen } },
        { provide: MatSnackBar, useValue: { open: vi.fn() } },
      ],
    })
      .overrideProvider(MatDialog, { useValue: { open: dialogOpen } })
      .compileComponents();

    const fixture = TestBed.createComponent(ReminderListComponent);
    fixture.detectChanges();

    await fixture.componentInstance['deleteReminder']({
      id: '1',
      message: 'Delete me',
      scheduledAt: new Date(),
      status: 'scheduled',
    });

    expect(dialogOpen).toHaveBeenCalledWith(
      ReminderDeleteDialogComponent,
      expect.objectContaining({ data: { message: 'Delete me' } }),
    );
    expect(deleteSpy).toHaveBeenCalledWith('1');
  });

  it('does not delete when the dialog is cancelled', async () => {
    const deleteSpy = vi.fn();
    const dialogOpen = vi.fn().mockReturnValue({
      afterClosed: () => of(false),
    });

    await TestBed.configureTestingModule({
      imports: [ReminderListComponent],
      providers: [
        { provide: ReminderService, useValue: createReminderServiceMock(deleteSpy) },
        { provide: ReminderHubService, useValue: createHubServiceMock() },
        { provide: MatDialog, useValue: { open: dialogOpen } },
        { provide: MatSnackBar, useValue: { open: vi.fn() } },
      ],
    })
      .overrideProvider(MatDialog, { useValue: { open: dialogOpen } })
      .compileComponents();

    const fixture = TestBed.createComponent(ReminderListComponent);
    fixture.detectChanges();

    await fixture.componentInstance['deleteReminder']({
      id: '1',
      message: 'Keep me',
      scheduledAt: new Date(),
      status: 'scheduled',
    });

    expect(deleteSpy).not.toHaveBeenCalled();
  });
});
