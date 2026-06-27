import { AbstractControl } from '@angular/forms';

import { scheduledInFutureValidator } from './scheduled-in-future.validator';

describe('scheduledInFutureValidator', () => {
  const validator = scheduledInFutureValidator();

  it('accepts a future date and time', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    const control = {
      get: (name: string) => ({
        value: name === 'date' ? tomorrow : '12:00',
      }),
    } as AbstractControl;

    expect(validator(control)).toBeNull();
  });

  it('rejects a past date and time', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);

    const control = {
      get: (name: string) => ({
        value: name === 'date' ? yesterday : '12:00',
      }),
    } as AbstractControl;

    expect(validator(control)).toEqual({ futureDateTime: true });
  });
});
