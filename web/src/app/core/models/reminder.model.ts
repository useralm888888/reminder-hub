export type ReminderStatus = 'scheduled' | 'sent';

export interface Reminder {
  id: string;
  message: string;
  scheduledAt: Date;
  email?: string;
  status: ReminderStatus;
}
