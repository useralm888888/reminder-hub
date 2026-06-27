export interface UpdateReminderPayload {
  message: string;
  scheduledAt: Date;
  email?: string;
}
