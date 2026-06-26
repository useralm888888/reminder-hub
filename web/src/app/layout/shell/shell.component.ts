import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';

import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  title: string;
  subtitle: string;
}

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatIconModule,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected readonly navItems: NavItem[] = [
    {
      label: 'Scheduling',
      route: '/scheduling',
      icon: 'calendar_month',
      title: 'Scheduling',
      subtitle: 'Enter a message, date, and an optional recipient email.',
    },
    {
      label: 'Reminder list',
      route: '/list',
      icon: 'notifications',
      title: 'Reminder list',
      subtitle: 'Overview of scheduled and sent reminders.',
    },
  ];

  protected readonly currentUsername = this.authService.currentUsername;

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(() => this.router.url),
      startWith(this.router.url),
    ),
    { initialValue: this.router.url },
  );

  protected readonly activePage = computed(() => {
    const url = this.currentUrl();
    return this.navItems.find((item) => url.startsWith(item.route)) ?? this.navItems[0];
  });

  protected logout(): void {
    this.authService.logout();
  }
}
