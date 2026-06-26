export interface UpdateReminderRequest {
  message: string;
  sendAt: string;
  email?: string | null;
}
