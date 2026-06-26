import { ReminderStatusApi } from '../enums/reminder-status.api';

export interface CreateReminderResponse {
  id: string;
  status: ReminderStatusApi;
  sendAt: string;
}
