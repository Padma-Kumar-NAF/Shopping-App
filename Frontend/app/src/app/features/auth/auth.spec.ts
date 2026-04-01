import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { Auth } from './auth';
import { AuthApiService } from '../../services/auth.service';
import { AuthStateService } from '../../services/auth-state.service';
import { RedirectService } from '../../services/redirect.service';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

describe('Auth', () => {
  let component: Auth;
  let fixture: ComponentFixture<Auth>;

  const mockToken = 'mock.jwt.token';
  const mockUser = { userRole: 'user', userId: '1', email: 'user@gmail.com', name: 'User' };

  const LoginApi = vi.fn();
  const SignUpApi = vi.fn();
  const decodeToken = vi.fn();
  const isAuthenticated = vi.fn().mockReturnValue(false);
  const setUser = vi.fn();
  const role = vi.fn().mockReturnValue('user');
  const hasPendingRedirect = vi.fn().mockReturnValue(false);
  const navigateToIntendedRoute = vi.fn();
  const navigate = vi.fn();

  beforeEach(async () => {
    vi.clearAllMocks();
    isAuthenticated.mockReturnValue(false);
    hasPendingRedirect.mockReturnValue(false);

    await TestBed.configureTestingModule({
      imports: [Auth, CommonModule, FormsModule, ReactiveFormsModule],
      providers: [
        { provide: AuthApiService, useValue: { LoginApi, SignUpApi, decodeToken } },
        { provide: AuthStateService, useValue: { isAuthenticated, setUser, role } },
        { provide: RedirectService, useValue: { hasPendingRedirect, navigateToIntendedRoute } },
        { provide: Router, useValue: { navigate } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Auth);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize in login mode', () => {
    expect(component.isLoginMode()).toBe(true);
  });

  it('should initialize loginForm and signUpForm', () => {
    expect(component.loginForm).toBeDefined();
    expect(component.signUpForm).toBeDefined();
  });

  describe('toggleMode', () => {
    it('should switch from login to signup mode', () => {
      component.toggleMode();
      expect(component.isLoginMode()).toBe(false);
    });

    it('should switch back to login mode', () => {
      component.toggleMode();
      component.toggleMode();
      expect(component.isLoginMode()).toBe(true);
    });
  });

  describe('Form getters', () => {
    it('getSignUpEmail should return email control', () => {
      expect(component.getSignUpEmail).toBeTruthy();
    });

    it('getSignUpName should return name control', () => {
      expect(component.getSignUpName).toBeTruthy();
    });

    it('getSignUpPassword should return password control', () => {
      expect(component.getSignUpPassword).toBeTruthy();
    });

    it('getUserEmail should return login email control', () => {
      expect(component.getUserEmail).toBeTruthy();
    });

    it('getLoginUserPassword should return login password control', () => {
      expect(component.getLoginUserPassword).toBeTruthy();
    });
  });

  describe('onLogin', () => {
    it('should call LoginApi and navigate to / on success for user role', () => {
      LoginApi.mockReturnValue(of({ data: { token: mockToken } }));
      decodeToken.mockReturnValue({ ...mockUser, userRole: 'user' });

      component.loginForm.setValue({ email: 'user@gmail.com', password: '##pk545A' });
      component.onLogin();

      expect(LoginApi).toHaveBeenCalled();
      expect(setUser).toHaveBeenCalled();
      expect(navigate).toHaveBeenCalledWith(['/']);
    });

    it('should navigate to /admin when role is admin', () => {
      LoginApi.mockReturnValue(of({ data: { token: mockToken } }));
      decodeToken.mockReturnValue({ ...mockUser, userRole: 'admin' });

      component.loginForm.setValue({ email: 'admin@gmail.com', password: '##pk545A' });
      component.onLogin();

      expect(navigate).toHaveBeenCalledWith(['/admin']);
    });

    it('should use redirect service when pending redirect exists', () => {
      hasPendingRedirect.mockReturnValue(true);
      LoginApi.mockReturnValue(of({ data: { token: mockToken } }));
      decodeToken.mockReturnValue(mockUser);

      component.loginForm.setValue({ email: 'user@gmail.com', password: '##pk545A' });
      component.onLogin();

      expect(navigateToIntendedRoute).toHaveBeenCalled();
    });

    it('should not call LoginApi when email is invalid', () => {
      component.loginForm.setValue({ email: 'not-an-email', password: '##pk545A' });
      component.onLogin();
      expect(LoginApi).not.toHaveBeenCalled();
    });
  });

  describe('onSignup', () => {
    const validSignup = {
      name: 'Test User',
      email: 'test@example.com',
      password: 'Password1',
      phoneNumber: '9876543210',
      addressLine1: '123 Main St',
      addressLine2: 'Apt 1',
      state: 'Tamil Nadu',
      city: 'Chennai',
      pincode: '600001',
    };

    it('should call SignUpApi and switch to login mode on success', () => {
      SignUpApi.mockReturnValue(of({}));
      component.signUpForm.setValue(validSignup);
      component.onSignup();
      expect(SignUpApi).toHaveBeenCalled();
      expect(component.isLoginMode()).toBe(true);
    });

    it('should not call SignUpApi when form is invalid', () => {
      component.signUpForm.reset();
      component.onSignup();
      expect(SignUpApi).not.toHaveBeenCalled();
    });
  });

  describe('redirect on already authenticated', () => {
    it('should redirect to / if already authenticated as user', () => {
      isAuthenticated.mockReturnValue(true);
      role.mockReturnValue('user');
      hasPendingRedirect.mockReturnValue(false);

      fixture = TestBed.createComponent(Auth);
      component = fixture.componentInstance;

      expect(navigate).toHaveBeenCalledWith(['/']);
    });
  });
});
