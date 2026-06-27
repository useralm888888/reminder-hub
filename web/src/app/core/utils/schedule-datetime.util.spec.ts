import { buildScheduledAt, formatTimeFromDate, isScheduleDateAllowed } from './schedule-datetime.util';

describe('schedule-datetime.util', () => {
  it('buildScheduledAt merges date and time in local timezone', () => {
    const date = new Date(2026, 5, 27);
    const scheduledAt = buildScheduledAt(date, '14:30');

    expect(scheduledAt.getFullYear()).toBe(2026);
    expect(scheduledAt.getMonth()).toBe(5);
    expect(scheduledAt.getDate()).toBe(27);
    expect(scheduledAt.getHours()).toBe(14);
    expect(scheduledAt.getMinutes()).toBe(30);
    expect(scheduledAt.getSeconds()).toBe(0);
  });

  it('formatTimeFromDate returns HH:mm', () => {
    const date = new Date(2026, 0, 1, 9, 5);

    expect(formatTimeFromDate(date)).toBe('09:05');
  });

  it('isScheduleDateAllowed rejects dates before today', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);

    expect(isScheduleDateAllowed(yesterday)).toBe(false);
    expect(isScheduleDateAllowed(new Date())).toBe(true);
  });
});
