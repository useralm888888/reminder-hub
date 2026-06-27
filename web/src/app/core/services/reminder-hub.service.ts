import { Injectable, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';

import { AppConfigService } from '../config/app-config.service';
import { ApiTokenService } from './api-token.service';

export interface ReminderStatusChangedEvent {
  id: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class ReminderHubService {
  private readonly appConfig = inject(AppConfigService);
  private readonly tokenService = inject(ApiTokenService);

  private connection: HubConnection | null = null;
  private onStatusChanged: ((event: ReminderStatusChangedEvent) => void) | null = null;

  readonly connectionFailed = signal(false);

  async start(onStatusChanged: (event: ReminderStatusChangedEvent) => void): Promise<void> {
    this.onStatusChanged = onStatusChanged;

    if (!this.tokenService.hasToken()) {
      return;
    }

    if (this.connection?.state === HubConnectionState.Connected) {
      return;
    }

    this.connection?.off(ReminderHubService.statusChangedMethod);
    await this.connection?.stop();

    this.connection = new HubConnectionBuilder()
      .withUrl(`${this.appConfig.apiBaseUrl}/hubs/reminders`, {
        accessTokenFactory: () => this.tokenService.getToken(),
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on(ReminderHubService.statusChangedMethod, (event: ReminderStatusChangedEvent) => {
      this.onStatusChanged?.(event);
    });

    try {
      await this.connection.start();
      this.connectionFailed.set(false);
    } catch {
      this.connectionFailed.set(true);
    }
  }

  async stop(): Promise<void> {
    this.onStatusChanged = null;
    if (this.connection) {
      this.connection.off(ReminderHubService.statusChangedMethod);
      await this.connection.stop();
      this.connection = null;
    }
  }

  private static readonly statusChangedMethod = 'ReminderStatusChanged';
}
