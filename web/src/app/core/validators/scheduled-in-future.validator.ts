import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

import { buildScheduledAt } from '../utils/schedule-datetime.util';

export const SCHEDULED_IN_FUTURE_MESSAGE =
  'Cannot schedule at the current time. Choose a date and time in the future.';

export function scheduledInFutureValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const date = control.get('date')?.value;
    const time = control.get('time')?.value;

    if (!date || !time) {
      return null;
    }

    const scheduledAt = buildScheduledAt(date as Date, time as string);

    return scheduledAt.getTime() > Date.now() ? null : { futureDateTime: true };
  };
}

export function shouldShowFutureDateTimeError(
  control: AbstractControl,
  submitAttempted: boolean,
): boolean {
  return (
    control.hasError('futureDateTime')
    && (submitAttempted || control.touched || control.dirty)
  );
}
