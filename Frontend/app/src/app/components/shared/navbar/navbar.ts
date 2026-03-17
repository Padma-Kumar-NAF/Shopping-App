import { Component, OnInit, signal, inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { filter } from 'rxjs/operators';
import { AuthStateService } from '../../../services/auth-state.service';

/** Routes where the navbar should be HIDDEN */
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

    /** Whether the navbar should be visible on the current route */
    showNavbar = signal<boolean>(true);

    /** Mobile hamburger menu open/close state */
    mobileMenuOpen = signal<boolean>(false);

    /** Expose auth state signals to the template */
    isAuthenticated = this.authState.isAuthenticated;
    username = this.authState.username;

    /** Two-way bound search query */
    searchQuery = '';

    ngOnInit(): void {
        // Sync auth state from storage on component init
        this.authState.syncFromStorage();

        // Evaluate route visibility on init
        this.evaluateRoute(this.router.url);

        // Reactively update on every navigation
        this.router.events
            .pipe(filter((e) => e instanceof NavigationEnd))
            .subscribe((e: NavigationEnd) => {
                this.evaluateRoute(e.urlAfterRedirects);
                // Close mobile menu on route change
                this.mobileMenuOpen.set(false);
            });
    }

    private evaluateRoute(url: string): void {
        const path = url.split('?')[0]; // strip query params
        const hidden = HIDDEN_ROUTES.some(
            (route) => path === route || path.startsWith(route + '/')
        );
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
