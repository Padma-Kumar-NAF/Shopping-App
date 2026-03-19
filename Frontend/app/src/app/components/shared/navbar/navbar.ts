import { Component, OnInit, signal, inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { filter } from 'rxjs/operators';
import { AuthStateService } from '../../../services/auth-state.service';
import { AuthApiService } from '../../../services/auth.service';

const HIDDEN_ROUTES = [
  '/profile',
  '/profile/orders',
  '/profile/address',
  '/profile/wishlist',
  '/cart',
  '/auth',
  '/admin',
];

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css'],
})
export class NavbarComponent implements OnInit {
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);
  private authState = inject(AuthStateService);
  private apiService: AuthApiService = inject(AuthApiService);

  showNavbar = signal<boolean>(true);

  mobileMenuOpen = signal<boolean>(false);

  isAuthenticated = this.authState.isAuthenticated;
  username = this.authState.username;
  searchQuery = '';

  ngOnInit(): void {
  // if (isPlatformBrowser(this.platformId)) {
  //   const token = localStorage.getItem('JWT-Token');

  //   this.authState.setUser(
  //     this.apiService.decodeToken(token ?? ""),
  //     token ?? ""
  //   );
  // }

  this.evaluateRoute(this.router.url);

  this.router.events
    .pipe(filter((e) => e instanceof NavigationEnd))
    .subscribe((e: NavigationEnd) => {
      this.evaluateRoute(e.urlAfterRedirects);
      this.mobileMenuOpen.set(false);
    });
}

  private evaluateRoute(url: string): void {
    const path = url.split('?')[0];
    const hidden = HIDDEN_ROUTES.some((route) => path === route || path.startsWith(route + '/'));
    this.showNavbar.set(!hidden);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  onSearch(): void {
    const q = this.searchQuery.trim();
    if (q) {
      this.router.navigate(['/products'], { queryParams: { q } });
      this.closeMobileMenu();
    }
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
    this.closeMobileMenu();
  }
}
