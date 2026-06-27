import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { HttpErrorContext } from '../../core/errors/http-error.context';
import { ReminderService } from '../../core/services/reminder.service';
import { SchedulingComponent } from './scheduling.component';

describe('SchedulingComponent', () => {
  it('shows a validation error when scheduled time is in the past', async () => {
    const reminderService = {
      create: vi.fn(),
    };
    const snackBar = { open: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [SchedulingComponent],
      providers: [
        provideNativeDateAdapter(),
        provideRouter([]),
        { provide: ReminderService, useValue: reminderService },
        { provide: MatSnackBar, useValue: snackBar },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(SchedulingComponent);
    const component = fixture.componentInstance;

    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);

    component['form'].patchValue({
      message: 'Past reminder',
      date: yesterday,
      time: '08:00',
    });
    component['form'].markAllAsTouched();
    fixture.detectChanges();

    await component['submit']();

    expect(reminderService.create).not.toHaveBeenCalled();
    expect(component['form'].hasError('futureDateTime')).toBe(true);
    expect(snackBar.open).toHaveBeenCalled();
    expect(component['submitAttempted']()).toBe(true);
  });

  it('creates a reminder when the form is valid', async () => {
    const reminderService = {
      create: vi.fn().mockReturnValue(
        of({
          id: '1',
          message: 'Future reminder',
          scheduledAt: new Date(),
          status: 'scheduled',
        }),
      ),
    };

    await TestBed.configureTestingModule({
      imports: [SchedulingComponent],
      providers: [
        provideNativeDateAdapter(),
        provideRouter([]),
        { provide: ReminderService, useValue: reminderService },
        { provide: MatSnackBar, useValue: { open: vi.fn() } },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(SchedulingComponent);
    const component = fixture.componentInstance;
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    component['form'].patchValue({
      message: 'Future reminder',
      date: tomorrow,
      time: '14:30',
      email: '',
    });

    await component['submit']();

    expect(reminderService.create).toHaveBeenCalledOnce();
  });

  it('shows a snackbar when create fails', async () => {
    const snackBar = { open: vi.fn() };
    const reminderService = {
      create: vi.fn().mockReturnValue(
        throwError(
          () => new HttpErrorContext(new HttpErrorResponse({ status: 500 })),
        ),
      ),
    };

    await TestBed.configureTestingModule({
      imports: [SchedulingComponent],
      providers: [
        provideNativeDateAdapter(),
        provideRouter([]),
        { provide: ReminderService, useValue: reminderService },
        { provide: MatSnackBar, useValue: snackBar },
      ],
    })
      .overrideProvider(MatSnackBar, { useValue: snackBar })
      .compileComponents();

    const fixture = TestBed.createComponent(SchedulingComponent);
    const component = fixture.componentInstance;

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    component['form'].patchValue({
      message: 'Future reminder',
      date: tomorrow,
      time: '14:30',
      email: 'user@example.com',
    });
    component['form'].updateValueAndValidity();

    await component['submit']();

    expect(snackBar.open).toHaveBeenCalled();
  });
});
