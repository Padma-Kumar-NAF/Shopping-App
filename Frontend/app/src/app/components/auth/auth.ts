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
} from '../../models/auth.model';
import { AuthApiService } from '../../services/api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './auth.html',
  styleUrls: ['./auth.css'],
})

export class Auth {
  disabledButton = signal<boolean>(true)
  private apiService: AuthApiService = inject(AuthApiService);

  isLoginMode = signal(true);

  loginData: LoginModel;
  signupData: SignupModel;

  userDetails: CreateUserResponseDTO;
  loginDetails: LoginResponseDTO;

  loginForm: FormGroup;
  signUpForm: FormGroup;

  constructor(private router : Router) {
    this.loginData = new LoginModel();
    this.signupData = new SignupModel();

    this.userDetails = new CreateUserResponseDTO();
    this.loginDetails = new LoginResponseDTO();

    this.loginForm = new FormGroup({
      email: new FormControl('padmakumar41759@gmail.com', [Validators.required, Validators.email]),
      password: new FormControl('##pk545A', [Validators.required, Validators.minLength(6)]),
    });

    this.signUpForm = new FormGroup({
      name: new FormControl('Test-username ', [Validators.required]),
      email: new FormControl('test@gmail.com', [Validators.required, Validators.email]),
      password: new FormControl('##pk545A', [Validators.required, Validators.minLength(6)]),
      phoneNumber: new FormControl('9876543210', [Validators.required, Validators.minLength(10)]),
      addressLine1: new FormControl('2/102 A Ragal bavi', [Validators.required]),
      addressLine2: new FormControl('S.K.Palayam(p.o)', [Validators.required]),
      state: new FormControl('Tamil nadu', [Validators.required]),
      city: new FormControl('Udumalpet', [Validators.required]),
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
    const toastId = toast.loading("Signing in...");
    this.apiService.LoginApi(this.loginData).subscribe({
      next: (response: LoginResponseDTO) => {
        this.loginDetails = response;
        localStorage.setItem('JWT-Token', response.token);
        this.loginForm.reset();
        this.router.navigate([''])
        console.log(this.loginDetails);
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
        toast.dismiss(toastId);
        toast.success('Login Successfull');
      },
    });
    console.log('Login Data:', this.loginForm.value);
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
    
    const toastId = toast.loading("Signing up...");

    this.apiService.SignUpApi(this.signupData).subscribe({
      next: (response: CreateUserResponseDTO) => {
        this.userDetails = response;
        toast.dismiss(toastId);
        toast.success('Signup Successfully');
        this.signUpForm.reset();
      },
      error: (error: any) => {
        if (error.status === 400 && error.error?.errors) {
          const serverErrors = error.error.errors;
          Object.keys(serverErrors).forEach((field) => {
            const messages = serverErrors[field];
            if (messages && messages.length > 0) {
              toast.error(messages[0]);
            }
          });
        } else if (error.error?.message) {
          toast.error(error.error.message);
        } else if (typeof error.error === 'string') {
          toast.error(error.error);
        } else {
          toast.error('Server error occurred');
        }
      },
    });
  }

  toggleMode() {
    this.isLoginMode.set(!this.isLoginMode());
  }
}