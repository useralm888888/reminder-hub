import { HttpErrorResponse } from '@angular/common/http';

interface ProblemDetails {
  title?: string;
  detail?: string;
}

export function mapHttpErrorMessage(
  error: HttpErrorResponse,
  fallback = 'Something went wrong. Please try again.',
): string {
  if (error.status === 0) {
    return 'Cannot reach the server. Is the API running?';
  }

  if (error.status === 401) {
    return 'Invalid or missing API token.';
  }

  if (error.status === 400) {
    const problem = error.error as ProblemDetails | null;
    const detail = problem?.detail ?? problem?.title;
    if (typeof detail === 'string' && detail.trim().length > 0) {
      return detail;
    }

    return 'The request was invalid. Check your input and try again.';
  }

  if (error.status >= 500) {
    return 'The server encountered an error. Please try again later.';
  }

  return fallback;
}
