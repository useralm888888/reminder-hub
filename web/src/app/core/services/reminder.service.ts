import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, map, tap, throwError } from 'rxjs';

import { ReminderDto } from '../api/dto/reminder.dto';
import { CreateReminderRequest } from '../api/requests/create-reminder.request';
import { UpdateReminderRequest } from '../api/requests/update-reminder.request';
import { CreateReminderResponse } from '../api/responses/create-reminder.response';
import { ReminderListResponse } from '../api/responses/reminder-list.response';
import { API_CONFIG } from '../config/api.config';
import { getErrorMessage } from '../errors/http-error.context';
import { mapReminderDto } from '../mappers/reminder.mapper';
import { Reminder } from '../models/reminder.model';

export interface CreateReminderPayload {
  message: string;
  scheduledAt: Date;
  email?: string;
}

export interface UpdateReminderPayload {
  message: string;
  scheduledAt: Date;
  email?: string;
}

@Injectable({ providedIn: 'root' })
export class ReminderService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(API_CONFIG);

  private readonly reminders = signal<Reminder[]>([]);
  private readonly loading = signal(false);
  private readonly error = signal<string | null>(null);
  private readonly page = signal(1);
  private readonly pageSize = signal(50);
  private readonly totalCount = signal(0);
  private readonly scheduledCount = signal(0);
  private readonly sentCount = signal(0);

  readonly items = this.reminders.asReadonly();
  readonly isLoading = this.loading.asReadonly();
  readonly loadError = this.error.asReadonly();
  readonly currentPage = this.page.asReadonly();
  readonly itemsPerPage = this.pageSize.asReadonly();
  readonly totalItems = this.totalCount.asReadonly();
  readonly scheduledTotal = this.scheduledCount.asReadonly();
  readonly sentTotal = this.sentCount.asReadonly();

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / this.pageSize())),
  );

  loadReminders(page = this.page()): Observable<Reminder[]> {
    this.loading.set(true);
    this.error.set(null);

    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', this.pageSize());

    return this.http
      .get<ReminderListResponse>(`${this.apiConfig.baseUrl}/reminders`, { params })
      .pipe(
        map((response) => {
          this.page.set(response.page);
          this.pageSize.set(response.pageSize);
          this.totalCount.set(response.totalCount);
          this.scheduledCount.set(response.scheduledCount);
          this.sentCount.set(response.sentCount);
          return response.items.map(mapReminderDto);
        }),
        tap((items) => this.reminders.set(items)),
        catchError((error) => {
          this.error.set(getErrorMessage(error, 'Failed to load reminders.'));
          this.reminders.set([]);
          return throwError(() => error);
        }),
        finalize(() => this.loading.set(false)),
      );
  }

  create(payload: CreateReminderPayload): Observable<Reminder> {
    const body: CreateReminderRequest = {
      message: payload.message,
      sendAt: payload.scheduledAt.toISOString(),
      email: payload.email ?? null,
    };

    return this.http
      .post<CreateReminderResponse>(`${this.apiConfig.baseUrl}/reminders`, body)
      .pipe(
        map((response) =>
          mapReminderDto({
            id: response.id,
            message: payload.message,
            sendAt: response.sendAt,
            status: response.status,
            email: payload.email ?? null,
          }),
        ),
      );
  }

  update(id: string, payload: UpdateReminderPayload): Observable<Reminder> {
    const body: UpdateReminderRequest = {
      message: payload.message,
      sendAt: payload.scheduledAt.toISOString(),
      email: payload.email ?? null,
    };

    return this.http
      .put<ReminderDto>(`${this.apiConfig.baseUrl}/reminders/${id}`, body)
      .pipe(map(mapReminderDto));
  }

  delete(id: string): Observable<void> {
    return this.http
      .delete<void>(`${this.apiConfig.baseUrl}/reminders/${id}`)
      .pipe(map(() => undefined));
  }
}
