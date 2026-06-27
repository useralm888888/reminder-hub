import { HttpErrorResponse } from '@angular/common/http';

import { mapHttpErrorMessage } from './http-error.mapper';

describe('mapHttpErrorMessage', () => {
  it('maps network failures', () => {
    const message = mapHttpErrorMessage(new HttpErrorResponse({ status: 0 }));

    expect(message).toContain('Cannot reach the server');
  });

  it('maps validation errors from ProblemDetails', () => {
    const message = mapHttpErrorMessage(
      new HttpErrorResponse({
        status: 400,
        error: {
          errors: {
            SendAt: ['SendAt must be a future date and time in UTC.'],
          },
        },
      }),
    );

    expect(message).toBe('SendAt must be a future date and time in UTC.');
  });

  it('maps unauthorized responses', () => {
    const message = mapHttpErrorMessage(new HttpErrorResponse({ status: 401 }));

    expect(message).toContain('Invalid or missing API token');
  });
});
