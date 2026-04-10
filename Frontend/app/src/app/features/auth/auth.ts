import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { toast } from 'ngx-sonner';
import {
  CreateUserResponseDTO,
  LoginModel,
  SignupModel,
  LoginResponseDTO,
} from '../../shared/models/users/auth.model';
import { AuthApiService } from '../../core/services/auth.service';
import { AuthStateService } from '../../core/state/auth-state.service';
import { RedirectService } from '../../core/services/redirect.service';
import { Router, RouterLink } from '@angular/router';
import { ApiResponse } from '../../shared/models/users/apiResponse.model';
import { INDIA_STATES, INDIA_STATES_CITIES } from '../../data/india-states-cities';

@Component({
  selector: 'app-auth',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './auth.html',
  styleUrls: ['./auth.css'],
})
export class Auth {
  private apiService: AuthApiService = inject(AuthApiService);
  private authState: AuthStateService = inject(AuthStateService);
  private redirectService: RedirectService = inject(RedirectService);

  states = INDIA_STATES;

  isLoginMode = signal(true);
  disabledButton = signal<boolean>(true);
  availableCities = signal<string[]>([]);
  showLoginPassword = signal(false);
  showRegisterPassword = signal(false);
  registerStep = signal<1 | 2>(1);

  loginData: LoginModel;
  signupData: SignupModel;

  loginDetails: LoginResponseDTO;

  loginForm: FormGroup;
  signUpForm: FormGroup;

  constructor(private router: Router) {
    if (this.authState.isAuthenticated()) {
      if (this.redirectService.hasPendingRedirect()) {
        this.redirectService.navigateToIntendedRoute();
      } else if (this.authState.role() === 'admin') {
        this.router.navigate(['/admin']);
      } else {
        this.router.navigate(['/']);
      }
    }

    this.loginData = new LoginModel();
    this.signupData = new SignupModel();

    this.loginDetails = new LoginResponseDTO();

    this.loginForm = new FormGroup({
      // email: new FormControl('admin@gmail.com', [Validators.required, Validators.email]),
      // password: new FormControl('admin123', [Validators.required, Validators.minLength(6)]),
      email: new FormControl('user@gmail.com', [Validators.required, Validators.email]),
      password: new FormControl('##pk545A', [Validators.required, Validators.minLength(6)]),
    });

    this.signUpForm = new FormGroup({
      name: new FormControl('user', [Validators.required]),
      email: new FormControl('user@gmail.com', [Validators.required, Validators.email]),
      password: new FormControl('##pk545A', [Validators.required, Validators.minLength(6)]),
      phoneNumber: new FormControl('9876543213', [Validators.required, Validators.minLength(10)]),
      addressLine1: new FormControl('2/102 A Chennai', [Validators.required]),
      addressLine2: new FormControl('Sriram gateway', [Validators.required]),
      state: new FormControl('', [Validators.required]),
      city: new FormControl('', [Validators.required]),
      pincode: new FormControl('654321', [Validators.required]),
    });
  }

  public get getSignUpEmail(): AbstractControl | null {
    return this.signUpForm.get('email');
  }

  public get getSignUpName(): AbstractControl | null {
    return this.signUpForm.get('name');
  }

  public get getSignUpPassword(): AbstractControl | null {
    return this.signUpForm.get('password');
  }

  public get getUserEmail(): AbstractControl | null {
    return this.loginForm.get('email');
  }

  public get getLoginUserPassword(): AbstractControl | null {
    return this.loginForm.get('password');
  }

  onLogin() {
    if (this.getUserEmail?.errors?.['required']) {
      toast.error('Email is required');
      return;
    }
    if (this.getUserEmail?.errors?.['email']) {
      toast.error('Invalid email format');
      return;
    }
    if (this.getLoginUserPassword?.errors?.['required']) {
      toast.error('Password is required');
      return;
    }
    this.loginData = this.loginForm.value;
    const toastId = toast.loading('Signing in...');
    this.apiService.LoginApi(this.loginData).subscribe({
      next: (response: ApiResponse<LoginResponseDTO>) => {
        console.log('Login response');
        console.log(response);
        if (response.data) {
          const user = this.apiService.decodeToken(response.data.token);
          if (response.data.token) {
            this.authState.setUser(user, response.data.token);
          }
          this.loginForm.reset();
          toast.dismiss(toastId);
          toast.success('Login Successful');

          if (this.redirectService.hasPendingRedirect()) {
            this.redirectService.navigateToIntendedRoute();
          } else {
            if (user?.userRole === 'admin') {
              this.router.navigate(['/admin']);
            } else {
              this.router.navigate(['/']);
            }
          }
        }
      },
      error: (error: any) => {
        toast.dismiss(toastId);
        console.error('Login Failed:', error);
        if (error.error?.message) {
          toast.error(error.error.message);
        } else {
          toast.error('Server error occurred');
        }
      },
      complete: () => {
        console.log('Login completed');
      },
    });
  }

  onSignup() {
    if (this.signUpForm.invalid) {
      Object.keys(this.signUpForm.controls).forEach((key) => {
        const control = this.signUpForm.get(key);
        if (control?.errors) {
          if (control.errors['required']) {
            toast.error(`${key} is required`);
          } else if (control.errors['email']) {
            toast.error(`Invalid email format`);
          } else if (control.errors['minlength']) {
            toast.error(
              `${key} must be at least ${control.errors['minlength'].requiredLength} characters`,
            );
          }
        }
      });
      return;
    }

    this.signupData = this.signUpForm.value;

    const toastId = toast.loading('Signing up...');

    this.apiService.SignUpApi(this.signupData).subscribe({
      next: (response: ApiResponse<CreateUserResponseDTO>) => {
        console.log("Signup Response")
        console.log(response)
        toast.dismiss(toastId);
        toast.success('Signup Successfully');
        this.signUpForm.reset();
        this.isLoginMode.set(true);
      },
      error: (error: any) => {
        toast.dismiss(toastId);
        console.error('Signup Failed:', error);
        if (error.error?.message) {
          toast.error(error.error.message);
        } else {
          toast.error('Server error occurred');
        }
      },
    });
  }

  onStateChange(event: Event): void {
    const state = (event.target as HTMLSelectElement).value;
    this.availableCities.set(INDIA_STATES_CITIES[state] ?? []);
    this.signUpForm.get('city')?.setValue('');
  }
  

  nextStep(): void {
    const { name, email, password, phoneNumber } = this.signUpForm.controls;
    if (name.invalid) { toast.error('Full name is required'); return; }
    if (email.invalid) { toast.error(email.errors?.['email'] ? 'Invalid email format' : 'Email is required'); return; }
    if (password.invalid) { toast.error(password.errors?.['minlength'] ? 'Password must be at least 6 characters' : 'Password is required'); return; }
    if (phoneNumber.invalid) { toast.error('Valid phone number is required'); return; }
    this.registerStep.set(2);
  }

  prevStep(): void {
    this.registerStep.set(1);
  }

  toggleMode() {
    this.isLoginMode.set(!this.isLoginMode());
    this.registerStep.set(1);
  }
}
