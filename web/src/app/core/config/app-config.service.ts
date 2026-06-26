import { Injectable } from '@angular/core';

export interface AppConfig {
  api: {
    baseUrl: string;
  };
}

const DEFAULT_CONFIG: AppConfig = {
  api: {
    baseUrl: 'http://localhost:5169',
  },
};

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private config: AppConfig = DEFAULT_CONFIG;

  async load(): Promise<void> {
    try {
      const response = await fetch('/config.json', { cache: 'no-store' });
      if (!response.ok) {
        return;
      }

      const loaded = (await response.json()) as Partial<AppConfig>;
      this.config = {
        api: {
          baseUrl: loaded.api?.baseUrl?.trim() || DEFAULT_CONFIG.api.baseUrl,
        },
      };
    } catch {
      // Keep defaults when config.json is unavailable (e.g. during tests).
    }
  }

  get apiBaseUrl(): string {
    return this.config.api.baseUrl;
  }
}
