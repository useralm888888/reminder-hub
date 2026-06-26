import { HttpErrorResponse } from '@angular/common/http';

import { mapHttpErrorMessage } from './http-error.mapper';

export class HttpErrorContext {
  readonly userMessage: string;

  constructor(readonly response: HttpErrorResponse) {
    this.userMessage = mapHttpErrorMessage(response);
  }
}

export function isHttpErrorContext(error: unknown): error is HttpErrorContext {
  return error instanceof HttpErrorContext;
}

export function getErrorMessage(error: unknown, fallback?: string): string {
  if (error instanceof HttpErrorContext) {
    return error.userMessage;
  }

  if (error instanceof HttpErrorResponse) {
    return mapHttpErrorMessage(error, fallback);
  }

  return fallback ?? 'Something went wrong. Please try again.';
}
