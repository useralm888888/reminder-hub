import { ReminderStatusApi } from '../enums/reminder-status.api';

export interface ReminderDto {
  id: string;
  message: string;
  sendAt: string;
  status: ReminderStatusApi;
  email: string | null;
}
