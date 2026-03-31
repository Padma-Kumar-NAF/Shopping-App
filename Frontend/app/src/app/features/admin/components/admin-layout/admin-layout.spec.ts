import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';
import { vi } from 'vitest';
import { AdminLayout } from './admin-layout';
import { AuthStateService } from '../../../services/auth-state.service';
import { StoreService } from '../../../services/adminServices/store.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('AdminLayout', () => {
  let component: AdminLayout;
  let fixture: ComponentFixture<AdminLayout>;
  let routerEvents$: Subject<any>;

  const navigate = vi.fn();
  const logout = vi.fn();
  const triggerRefresh = vi.fn();

  beforeEach(async () => {
    routerEvents$ = new Subject();
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [AdminLayout],
      providers: [
        { provide: Router, useValue: { navigate, url: '/admin/dashboard', events: routerEvents$.asObservable() } },
        { provide: AuthStateService, useValue: { logout } },
        { provide: StoreService, useValue: { triggerRefresh } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have 7 nav items', () => {
    expect(component.navItems.length).toBe(7);
  });

  it('should set currentRoute on init', () => {
    expect(component.currentRoute()).toBe('/admin/dashboard');
  });

  it('should update currentRoute on NavigationEnd', () => {
    routerEvents$.next(new NavigationEnd(1, '/admin/users', '/admin/users'));
    expect(component.currentRoute()).toBe('/admin/users');
  });

  describe('isActive', () => {
    it('should return true when route starts with path', () => {
      component.currentRoute.set('/admin/dashboard');
      expect(component.isActive('/admin/dashboard')).toBe(true);
    });

    it('should return false when route does not match', () => {
      component.currentRoute.set('/admin/users');
      expect(component.isActive('/admin/dashboard')).toBe(false);
    });
  });

  describe('currentPageLabel', () => {
    it('should return matching nav item label', () => {
      component.currentRoute.set('/admin/users');
      expect(component.currentPageLabel()).toBe('Users');
    });

    it('should return Dashboard as default', () => {
      component.currentRoute.set('/admin/unknown');
      expect(component.currentPageLabel()).toBe('Dashboard');
    });
  });

  describe('toggleMobileMenu', () => {
    it('should toggle mobileMenuOpen', () => {
      expect(component.mobileMenuOpen()).toBe(false);
      component.toggleMobileMenu();
      expect(component.mobileMenuOpen()).toBe(true);
    });
  });

  describe('closeMobileMenu', () => {
    it('should set mobileMenuOpen to false', () => {
      component.mobileMenuOpen.set(true);
      component.closeMobileMenu();
      expect(component.mobileMenuOpen()).toBe(false);
    });
  });

  describe('logout', () => {
    it('should call authState.logout', () => {
      component.logout();
      expect(logout).toHaveBeenCalled();
    });
  });
});
