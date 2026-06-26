import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      {
        path: '',
        redirectTo: 'scheduling',
        pathMatch: 'full',
      },
      {
        path: 'scheduling',
        loadComponent: () =>
          import('./features/scheduling/scheduling.component').then((m) => m.SchedulingComponent),
      },
      {
        path: 'list',
        loadComponent: () =>
          import('./features/reminder-list/reminder-list.component').then(
            (m) => m.ReminderListComponent,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
