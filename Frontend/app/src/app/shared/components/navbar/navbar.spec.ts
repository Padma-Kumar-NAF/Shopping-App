import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router, NavigationEnd } from '@angular/router';
import { Location } from '@angular/common';
import { of, Subject } from 'rxjs';
import { PLATFORM_ID, signal } from '@angular/core';
import { vi } from 'vitest';
import { NavbarComponent } from './navbar';
import { AuthStateService } from '../../../services/auth-state.service';
import { AuthApiService } from '../../../services/auth.service';
import { ProductSuggestionService } from '../../../services/product-suggestion.service';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let routerEvents$: Subject<any>;

  const navigate = vi.fn();
  const back = vi.fn();
  const getSuggestions = vi.fn().mockReturnValue(of([]));

  beforeEach(async () => {
    routerEvents$ = new Subject();
    vi.clearAllMocks();
    getSuggestions.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        { provide: Router, useValue: { navigate, url: '/', events: routerEvents$.asObservable() } },
        { provide: Location, useValue: { back } },
        { provide: PLATFORM_ID, useValue: 'browser' },
        { provide: AuthStateService, useValue: { isAuthenticated: signal(false), username: signal('') } },
        { provide: AuthApiService, useValue: {} },
        { provide: ProductSuggestionService, useValue: { getSuggestions } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Route evaluation', () => {
    it('should hide navbar on /cart route', () => {
      routerEvents$.next(new NavigationEnd(1, '/cart', '/cart'));
      expect(component.showNavbar()).toBe(false);
    });

    it('should hide navbar on /auth route', () => {
      routerEvents$.next(new NavigationEnd(1, '/auth', '/auth'));
      expect(component.showNavbar()).toBe(false);
    });

    it('should show navbar on /products route', () => {
      routerEvents$.next(new NavigationEnd(1, '/products', '/products'));
      expect(component.showNavbar()).toBe(true);
    });

    it('should show back button on non-root routes', () => {
      routerEvents$.next(new NavigationEnd(1, '/products', '/products'));
      expect(component.showBackButton()).toBe(true);
    });

    it('should not show back button on root route', () => {
      routerEvents$.next(new NavigationEnd(1, '/', '/'));
      expect(component.showBackButton()).toBe(false);
    });
  });

  describe('Mobile menu', () => {
    it('toggleMobileMenu should toggle mobileMenuOpen', () => {
      expect(component.mobileMenuOpen()).toBe(false);
      component.toggleMobileMenu();
      expect(component.mobileMenuOpen()).toBe(true);
      component.toggleMobileMenu();
      expect(component.mobileMenuOpen()).toBe(false);
    });

    it('closeMobileMenu should set mobileMenuOpen to false', () => {
      component.mobileMenuOpen.set(true);
      component.closeMobileMenu();
      expect(component.mobileMenuOpen()).toBe(false);
    });
  });

  describe('Search', () => {
    it('onSearch should navigate to /products with query', () => {
      component.searchQuery = 'laptop';
      component.onSearch();
      expect(navigate).toHaveBeenCalledWith(['/products'], { queryParams: { q: 'laptop' } });
    });

    it('onSearch should not navigate when query is empty', () => {
      component.searchQuery = '   ';
      component.onSearch();
      expect(navigate).not.toHaveBeenCalled();
    });

    it('selectSuggestion should set query and navigate', () => {
      component.selectSuggestion('phone');
      expect(component.searchQuery).toBe('phone');
      expect(navigate).toHaveBeenCalledWith(['/products'], { queryParams: { q: 'phone' } });
    });

    it('closeSuggestions should clear suggestions and hide dropdown', () => {
      component.suggestions.set(['a', 'b']);
      component.showSuggestions.set(true);
      component.closeSuggestions();
      expect(component.suggestions()).toEqual([]);
      expect(component.showSuggestions()).toBe(false);
    });

    it('onSearchInput should push value to typeahead', fakeAsync(() => {
      getSuggestions.mockReturnValue(of(['phone', 'laptop']));
      component.onSearchInput('phone');
      tick(300);
      expect(getSuggestions).toHaveBeenCalledWith('phone');
    }));
  });

  describe('Navigation', () => {
    it('navigateTo should navigate to given path and close mobile menu', () => {
      component.mobileMenuOpen.set(true);
      component.navigateTo('/profile');
      expect(navigate).toHaveBeenCalledWith(['/profile']);
      expect(component.mobileMenuOpen()).toBe(false);
    });

    it('goBack should call location.back when history exists', () => {
      vi.spyOn(window, 'history', 'get').mockReturnValue({ length: 3 } as History);
      component.goBack();
      expect(back).toHaveBeenCalled();
    });

    it('goBack should navigate to / when no history', () => {
      vi.spyOn(window, 'history', 'get').mockReturnValue({ length: 1 } as History);
      component.goBack();
      expect(navigate).toHaveBeenCalledWith(['/']);
    });
  });

  describe('ngOnDestroy', () => {
    it('should complete destroy$ on destroy', () => {
      const nextSpy = vi.spyOn((component as any).destroy$, 'next');
      const completeSpy = vi.spyOn((component as any).destroy$, 'complete');
      component.ngOnDestroy();
      expect(nextSpy).toHaveBeenCalled();
      expect(completeSpy).toHaveBeenCalled();
    });
  });
});
