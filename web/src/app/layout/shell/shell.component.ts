import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
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
  private readonly breakpointObserver = inject(BreakpointObserver);

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
  protected readonly isMobile = signal(false);
  protected readonly sidebarOpen = signal(true);

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

  constructor() {
    this.breakpointObserver
      .observe([Breakpoints.Handset])
      .pipe(takeUntilDestroyed())
      .subscribe((result) => {
        const mobile = result.matches;
        this.isMobile.set(mobile);
        this.sidebarOpen.set(!mobile);
      });
  }

  protected openSidebar(): void {
    this.sidebarOpen.set(true);
  }

  protected closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  protected closeSidebarOnNavigate(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  protected logout(): void {
    this.authService.logout();
  }
}
