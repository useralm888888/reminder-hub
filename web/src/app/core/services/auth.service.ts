import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';

import { LoginRequest } from '../api/requests/login.request';
import { LoginResponse } from '../api/responses/login.response';
import { API_CONFIG } from '../config/api.config';
import { ApiTokenService } from './api-token.service';

const USERNAME_STORAGE_KEY = 'reminder-hub-username';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(API_CONFIG);
  private readonly apiTokenService = inject(ApiTokenService);
  private readonly router = inject(Router);

  private readonly username = signal(this.readUsername());

  readonly currentUsername = this.username.asReadonly();
  readonly isAuthenticated = this.apiTokenService.hasToken;

  login(credentials: LoginRequest): Observable<void> {
    return this.http
      .post<LoginResponse>(`${this.apiConfig.baseUrl}/auth/login`, credentials)
      .pipe(
        tap((response) => this.persistSession(response.token, response.username)),
        map(() => undefined),
      );
  }

  logout(): void {
    this.apiTokenService.clearToken();
    sessionStorage.removeItem(USERNAME_STORAGE_KEY);
    this.username.set(null);
    void this.router.navigate(['/login']);
  }

  private persistSession(token: string, username: string): void {
    this.apiTokenService.setToken(token);
    sessionStorage.setItem(USERNAME_STORAGE_KEY, username);
    this.username.set(username);
  }

  private readUsername(): string | null {
    return sessionStorage.getItem(USERNAME_STORAGE_KEY);
  }
}
