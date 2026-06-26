import { ReminderDto } from '../dto/reminder.dto';

export interface ReminderListResponse {
  items: ReminderDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  scheduledCount: number;
  sentCount: number;
}
