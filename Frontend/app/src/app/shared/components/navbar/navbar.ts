import { Component, OnInit, OnDestroy, signal, inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { CommonModule, isPlatformBrowser, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { filter, takeUntil, debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { AuthStateService } from '../../../core/state/auth-state.service';
import { AuthApiService } from '../../../core/services/auth.service';
import { ProductSuggestionService } from '../../../features/user/services/product-suggestion.service';

const HIDDEN_ROUTES = ['/cart', '/auth', '/admin'];
const NO_BACK_ROUTES = ['/'];

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css'],
})
export class NavbarComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private location = inject(Location);
  private authState = inject(AuthStateService);
  private suggestionService = inject(ProductSuggestionService);

  showNavbar = signal<boolean>(true);
  showBackButton = signal<boolean>(false);
  mobileMenuOpen = signal<boolean>(false);

  isAuthenticated = this.authState.isAuthenticated;
  username = this.authState.username;
  searchQuery = '';

  suggestions = signal<string[]>([]);
  showSuggestions = signal<boolean>(false);

  private typeahead$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // console.log(window.history)
    // console.log(window.history.length)
    this.evaluateRoute(this.router.url);

    this.router.events.pipe(filter((e) => e instanceof NavigationEnd), takeUntil(this.destroy$))
      .subscribe((e: NavigationEnd) => {
        this.evaluateRoute(e.urlAfterRedirects);
        this.mobileMenuOpen.set(false);
        this.closeSuggestions();
      });
    this.typeahead$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((q) => {
        if (!q.trim()) {
          this.closeSuggestions();
          return [];
        }
        return this.suggestionService.getSuggestions(q);
      }),
      takeUntil(this.destroy$),
    ).subscribe((results) => {
      this.suggestions.set(results);
      this.showSuggestions.set(results.length > 0);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchInput(value: string): void {
    this.searchQuery = value;
    this.typeahead$.next(value);
  }

  selectSuggestion(name: string): void {
    this.searchQuery = name;
    this.closeSuggestions();
    this.router.navigate(['/products'], { queryParams: { q: name } });
    this.closeMobileMenu();
  }

  closeSuggestions(): void {
    this.showSuggestions.set(false);
    this.suggestions.set([]);
  }

  onSearch(): void {
    const q = this.searchQuery.trim();
    this.closeSuggestions();
    if (q) {
      this.router.navigate(['/products'], { queryParams: { q } });
      this.closeMobileMenu();
    }
  }

  private evaluateRoute(url: string): void {
    const path = url.split('?')[0];
    const hidden = HIDDEN_ROUTES.some((route) => path === route || path.startsWith(route + '/'));
    this.showNavbar.set(!hidden);
    this.showBackButton.set(!hidden && !NO_BACK_ROUTES.includes(path));
  }

  goBack(): void {
    if (window.history.length > 1) {
      this.location.back();
    } else {
      this.router.navigate(['/']);
    }
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
    this.closeMobileMenu();
  }
}
