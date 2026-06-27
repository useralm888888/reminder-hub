import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { API_CONFIG } from '../config/api.config';
import { ReminderService } from './reminder.service';

describe('ReminderService', () => {
  let service: ReminderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ReminderService,
        {
          provide: API_CONFIG,
          useValue: { baseUrl: 'http://localhost:5169' },
        },
      ],
    });

    service = TestBed.inject(ReminderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('loads reminders and updates pagination state', async () => {
    const loadPromise = firstValueFrom(service.loadReminders(1));

    const request = httpMock.expectOne('http://localhost:5169/reminders?page=1&pageSize=50');
    request.flush({
      items: [
        {
          id: '1',
          message: 'Test',
          sendAt: '2026-06-27T14:30:00Z',
          status: 'Scheduled',
          email: null,
        },
      ],
      page: 1,
      pageSize: 50,
      totalCount: 1,
      scheduledCount: 1,
      sentCount: 0,
    });

    const items = await loadPromise;

    expect(items).toHaveLength(1);
    expect(items[0].message).toBe('Test');
    expect(service.currentPage()).toBe(1);
    expect(service.totalItems()).toBe(1);
  });

  it('ignores stale responses when a newer request finishes first', async () => {
    const firstPromise = firstValueFrom(service.loadReminders(1));
    const secondPromise = firstValueFrom(service.loadReminders(2));

    const firstRequest = httpMock.expectOne('http://localhost:5169/reminders?page=1&pageSize=50');
    const secondRequest = httpMock.expectOne('http://localhost:5169/reminders?page=2&pageSize=50');

    secondRequest.flush({
      items: [
        {
          id: '2',
          message: 'Page 2',
          sendAt: '2026-06-27T14:30:00Z',
          status: 'Scheduled',
          email: null,
        },
      ],
      page: 2,
      pageSize: 50,
      totalCount: 2,
      scheduledCount: 2,
      sentCount: 0,
    });

    firstRequest.flush({
      items: [
        {
          id: '1',
          message: 'Page 1',
          sendAt: '2026-06-27T14:30:00Z',
          status: 'Scheduled',
          email: null,
        },
      ],
      page: 1,
      pageSize: 50,
      totalCount: 2,
      scheduledCount: 2,
      sentCount: 0,
    });

    await firstPromise;
    await secondPromise;

    expect(service.currentPage()).toBe(2);
    expect(service.items()[0].message).toBe('Page 2');
  });
});
