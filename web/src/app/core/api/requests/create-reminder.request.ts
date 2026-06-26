export interface CreateReminderRequest {
  message: string;
  sendAt: string;
  email?: string | null;
}
