import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterOutlet, NavigationEnd } from '@angular/router';
import { AuthStateService } from '../../../services/auth-state.service';
import { toast } from 'ngx-sonner';
import { filter } from 'rxjs/operators';

interface NavItem {
  path: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout implements OnInit {
  private router = inject(Router);
  private authState = inject(AuthStateService);

  mobileMenuOpen = signal<boolean>(false);
  currentRoute = signal<string>('');

  navItems: NavItem[] = [
    { path: '/admin/dashboard', label: 'Dashboard', icon: '📊' },
    { path: '/admin/users', label: 'Users', icon: '👥' },
    { path: '/admin/orders', label: 'Orders', icon: '📦' },
    { path: '/admin/products', label: 'Products', icon: '🛍️' },
    { path: '/admin/categories', label: 'Categories', icon: '🏷️' },
    { path: '/admin/promocodes', label: 'Promo Codes', icon: '🎟️' },
    { path: '/admin/error-logs', label: 'Error Logs', icon: '🪲' },
  ];

  ngOnInit(): void {
    // Track current route
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.currentRoute.set(event.url);
      });

    // Set initial route
    this.currentRoute.set(this.router.url);
  }

  isActive(path: string): boolean {
    return this.currentRoute().startsWith(path);
  }

  currentPageLabel(): string {
    const match = this.navItems.find(i => this.currentRoute().startsWith(i.path));
    return match?.label ?? 'Dashboard';
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  logout(): void {
    this.authState.clearUser();
    toast.success('Logged out successfully');
    this.router.navigate(['/auth']);
  }
}
