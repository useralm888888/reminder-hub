export function buildScheduledAt(date: Date, time: string): Date {
  const [hours, minutes] = time.split(':').map(Number);
  const scheduledAt = new Date(date);
  scheduledAt.setHours(hours, minutes, 0, 0);
  return scheduledAt;
}

export function startOfToday(): Date {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return today;
}

export function isScheduleDateAllowed(date: Date | null): boolean {
  if (date === null) {
    return false;
  }

  const candidate = new Date(date);
  candidate.setHours(0, 0, 0, 0);

  return candidate.getTime() >= startOfToday().getTime();
}

export function formatTimeFromDate(date: Date): string {
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}
