import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { API_CONFIG } from '../config/api.config';
import { ApiTokenService } from '../services/api-token.service';

export const apiTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const apiConfig = inject(API_CONFIG);
  const tokenService = inject(ApiTokenService);
  const remindersUrl = `${apiConfig.baseUrl}/reminders`;
  const requiresToken =
    (req.method === 'POST' && req.url === remindersUrl) ||
    (req.method === 'PUT' && req.url.startsWith(`${remindersUrl}/`)) ||
    (req.method === 'DELETE' && req.url.startsWith(`${remindersUrl}/`));

  if (requiresToken) {
    const token = tokenService.getToken();
    if (token) {
      return next(
        req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        }),
      );
    }
  }

  return next(req);
};
