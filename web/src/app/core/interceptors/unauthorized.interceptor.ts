import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { API_CONFIG } from '../config/api.config';
import { ApiTokenService } from '../services/api-token.service';

export const unauthorizedInterceptor: HttpInterceptorFn = (req, next) => {
  const apiConfig = inject(API_CONFIG);
  const tokenService = inject(ApiTokenService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse
        && error.status === 401
        && req.url.startsWith(apiConfig.baseUrl)
        && !req.url.includes('/auth/login')
      ) {
        tokenService.clearToken();
        void router.navigate(['/login']);
      }

      return throwError(() => error);
    }),
  );
};
