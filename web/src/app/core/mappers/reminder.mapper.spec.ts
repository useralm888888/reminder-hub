import { ReminderStatusApi } from '../api/enums/reminder-status.api';
import { ReminderDto } from '../api/dto/reminder.dto';
import { mapReminderDto, mapReminderStatus } from './reminder.mapper';

describe('reminder.mapper', () => {
  it('maps sent status', () => {
    expect(mapReminderStatus('Sent')).toBe('sent');
    expect(mapReminderStatus('Scheduled')).toBe('scheduled');
  });

  it('maps reminder dto to domain model', () => {
    const dto: ReminderDto = {
      id: 'abc',
      message: 'Check logs',
      sendAt: '2026-06-27T14:30:00Z',
      status: 'Scheduled' as ReminderStatusApi,
      email: 'user@example.com',
    };

    const reminder = mapReminderDto(dto);

    expect(reminder.id).toBe('abc');
    expect(reminder.message).toBe('Check logs');
    expect(reminder.status).toBe('scheduled');
    expect(reminder.email).toBe('user@example.com');
    expect(reminder.scheduledAt.toISOString()).toBe('2026-06-27T14:30:00.000Z');
  });
});
