import { Injectable, computed, signal } from '@angular/core';

const STORAGE_KEY = 'reminder-hub-api-token';

@Injectable({ providedIn: 'root' })
export class ApiTokenService {
  private readonly token = signal(this.readFromStorage());

  readonly value = this.token.asReadonly();
  readonly hasToken = computed(() => this.token().length > 0);

  getToken(): string {
    return this.token();
  }

  setToken(value: string): void {
    const trimmed = value.trim();
    sessionStorage.setItem(STORAGE_KEY, trimmed);
    this.token.set(trimmed);
  }

  clearToken(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this.token.set('');
  }

  private readFromStorage(): string {
    return sessionStorage.getItem(STORAGE_KEY) ?? '';
  }
}
