import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

import { buildScheduledAt } from '../utils/schedule-datetime.util';

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

export function hasFutureDateTimeError(control: AbstractControl): boolean {
  return control.hasError('futureDateTime') && (control.touched || control.dirty);
}
