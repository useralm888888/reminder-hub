import { inject, InjectionToken } from '@angular/core';

import { AppConfigService } from './app-config.service';

export interface ApiConfig {
  baseUrl: string;
}

export const API_CONFIG = new InjectionToken<ApiConfig>('API_CONFIG', {
  providedIn: 'root',
  factory: () => {
    const appConfig = inject(AppConfigService);
    return {
      baseUrl: appConfig.apiBaseUrl,
    };
  },
});
