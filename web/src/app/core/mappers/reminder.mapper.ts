import { ReminderStatusApi } from '../api/enums/reminder-status.api';
import { ReminderDto } from '../api/dto/reminder.dto';
import { Reminder, ReminderStatus } from '../models/reminder.model';

export function mapReminderStatus(status: ReminderStatusApi): ReminderStatus {
  return status === 'Sent' ? 'sent' : 'scheduled';
}

export function mapReminderDto(dto: ReminderDto): Reminder {
  return {
    id: dto.id,
    message: dto.message,
    scheduledAt: new Date(dto.sendAt),
    status: mapReminderStatus(dto.status),
    email: dto.email ?? undefined,
  };
}
