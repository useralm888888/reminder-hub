export interface CreateReminderPayload {
  message: string;
  scheduledAt: Date;
  email?: string;
}
