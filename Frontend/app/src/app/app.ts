import { Component, signal, inject, OnInit } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { NgxSonnerToaster } from 'ngx-sonner';
import { NavbarComponent } from './components/shared/navbar/navbar';
import { LoaderService } from './services/loading.service';
import { Spinner } from './components/spinner/spinner';
import { OrdersComponent } from './components/profileComponents/orders/orders';
import { AdminDashboard } from './components/adminComponents/admin-dashboard/admin-dashboard';
import { HomeComponent } from './components/homeComponents/home/home';
import { filter } from 'rxjs/operators';
import { AuthStateService } from './services/auth-state.service';

const NO_NAV_PREFIXES = ['/admin', '/auth', '/cart'];

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    NgxSonnerToaster,
    NavbarComponent,
    Spinner,
    OrdersComponent,
    AdminDashboard,
    HomeComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  protected readonly title = signal('app');
  loader = inject(LoaderService);
  private router = inject(Router);
  private authState = inject(AuthStateService);

  showNavPadding = signal(true);

  ngOnInit(): void {
    // Restore session and schedule auto-logout on every page load/refresh
    this.authState.loadUserFromStorage();

    this.evaluate(this.router.url);
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe((e: NavigationEnd) => this.evaluate(e.urlAfterRedirects));
  }

  private evaluate(url: string): void {
    const path = url.split('?')[0];
    const hidden = NO_NAV_PREFIXES.some(p => path === p || path.startsWith(p + '/'));
    this.showNavPadding.set(!hidden);
  }
}
