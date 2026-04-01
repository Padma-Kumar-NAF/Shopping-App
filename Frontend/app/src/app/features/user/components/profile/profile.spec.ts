import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { of, throwError, Subject } from 'rxjs';
import { vi } from 'vitest';
import { Profile } from './profile';
import { ProfileApiService } from '../../../services/profile.service';
import { AuthStateService } from '../../../services/auth-state.service';
import { WalletService } from '../../../services/userServices/wallet.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('Profile', () => {
  let component: Profile;
  let fixture: ComponentFixture<Profile>;
  let routerEvents$: Subject<any>;

  const navigate = vi.fn();
  const GetUserProfile = vi.fn();
  const updateUserDetails = vi.fn();
  const updateUserEmail = vi.fn();
  const logout = vi.fn();

  const mockUser: any = {
    userId: 'u1',
    name: 'Test User',
    email: 'test@example.com',
    userDetails: {
      phoneNumber: '9876543210',
      addressLine1: '123 Main St',
      addressLine2: '',
      state: 'TN',
      city: 'Chennai',
      pincode: '600001',
    },
  };

  beforeEach(async () => {
    routerEvents$ = new Subject();
    vi.clearAllMocks();
    GetUserProfile.mockReturnValue(of({ data: mockUser }));

    await TestBed.configureTestingModule({
      imports: [Profile],
      providers: [
        { provide: Router, useValue: { navigate, events: routerEvents$.asObservable() } },
        { provide: ActivatedRoute, useValue: { firstChild: null } },
        { provide: ProfileApiService, useValue: { GetUserProfile, updateUserDetails, updateUserEmail } },
        { provide: AuthStateService, useValue: { logout } },
        { provide: WalletService, useValue: { getWalletAmount: vi.fn() } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(Profile);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load user profile on init', () => {
    expect(GetUserProfile).toHaveBeenCalled();
    expect(component.user()).toEqual(mockUser);
    expect(component.isLoading()).toBe(false);
  });

  it('should set isLoading to false on profile load error', () => {
    GetUserProfile.mockReturnValue(throwError(() => ({ error: { message: 'Error' } })));
    component.GetUserProfileDetails();
    expect(component.isLoading()).toBe(false);
  });

  describe('tabs', () => {
    it('should have 5 tabs defined', () => {
      expect(component.tabs.length).toBe(5);
    });

    it('activeTabConfig should return config for active tab', () => {
      component.activeTab.set('cart');
      const config = component.activeTabConfig();
      expect(config.id).toBe('cart');
    });
  });

  describe('setTab', () => {
    it('should navigate to /profile for profile tab', () => {
      component.setTab('profile');
      expect(navigate).toHaveBeenCalledWith(['/profile']);
    });

    it('should navigate to /profile/:tab for other tabs', () => {
      component.setTab('cart');
      expect(navigate).toHaveBeenCalledWith(['/profile', 'cart']);
    });
  });

  describe('toggleMobileMenu', () => {
    it('should toggle mobileMenuOpen', () => {
      expect(component.mobileMenuOpen()).toBe(false);
      component.toggleMobileMenu();
      expect(component.mobileMenuOpen()).toBe(true);
    });
  });

  describe('Details modal', () => {
    it('openDetailsModal should patch form and show modal', () => {
      component.openDetailsModal();
      expect(component.showDetailsModal()).toBe(true);
      expect(component.userDetailsForm.value.name).toBe('Test User');
    });

    it('closeDetailsModal should hide modal', () => {
      component.showDetailsModal.set(true);
      component.closeDetailsModal();
      expect(component.showDetailsModal()).toBe(false);
    });

    it('saveDetails should call updateUserDetails on valid form', () => {
      updateUserDetails.mockReturnValue(of({}));
      component.openDetailsModal();
      component.saveDetails();
      expect(updateUserDetails).toHaveBeenCalled();
    });

    it('saveDetails should not call API when form is invalid', () => {
      component.userDetailsForm.reset();
      component.saveDetails();
      expect(updateUserDetails).not.toHaveBeenCalled();
    });
  });

  describe('Email modal', () => {
    it('openEmailModal should show modal', () => {
      component.openEmailModal();
      expect(component.showEmailModal()).toBe(true);
    });

    it('closeEmailModal should hide modal and reset', () => {
      component.showEmailModal.set(true);
      component.closeEmailModal();
      expect(component.showEmailModal()).toBe(false);
      expect(component.passwordVerified).toBe(false);
    });

    it('updateEmail should show error when password is empty', () => {
      component.editMail.password = '';
      component.updateEmail();
      expect(updateUserEmail).not.toHaveBeenCalled();
    });

    it('updateEmail should show error when new email is invalid', () => {
      component.editMail.password = 'pass123';
      component.editMail.newEmail = 'not-an-email';
      component.updateEmail();
      expect(updateUserEmail).not.toHaveBeenCalled();
    });

    it('updateEmail should call API with valid data', () => {
      updateUserEmail.mockReturnValue(of({ data: {} }));
      component.editMail.password = 'pass123';
      component.editMail.newEmail = 'new@example.com';
      component.updateEmail();
      expect(updateUserEmail).toHaveBeenCalled();
    });
  });

  describe('logout', () => {
    it('should call authState.logout', () => {
      component.logout();
      expect(logout).toHaveBeenCalled();
    });
  });
});
