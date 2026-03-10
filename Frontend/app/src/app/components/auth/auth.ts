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
} from '../../models/login.model';
import { AuthApiService } from '../../services/api.service';

@Component({
  selector: 'app-auth',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './auth.html',
  styleUrls: ['./auth.css'],
})
export class Auth {
  private apiService: AuthApiService = inject(AuthApiService);
  constructor() {
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
    });
  }

  isLoginMode = signal(true);

  loginData: LoginModel;
  signupData: SignupModel;

  userDetails: CreateUserResponseDTO;
  loginDetails: LoginResponseDTO;

  loginForm: FormGroup;
  signUpForm: FormGroup;

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
    this.apiService.LoginApi(this.loginData).subscribe({
      next: (response: LoginResponseDTO) => {
        this.loginDetails = response;
        sessionStorage.setItem('JWT-Token', response.token);
        // console.log("Stored Token:", sessionStorage.getItem('JWT-Token'));

        console.log(this.loginDetails);
      },
      error: (error: any) => {
        console.error('Login Failed:', error);

        if (error.error?.message) {
          toast.error(error.error.message);
        } else {
          toast.error('Server error occurred');
        }
      },
      complete: () => {
        toast.success('Login Successfull');
      },
    });
    console.log('Login Data:', this.loginForm.value);
    this.loginForm.reset();
  }

  onSignup() {
    if (this.getSignUpEmail?.errors?.['required']) {
      toast.error('Email is required');
      return;
    }

    if (this.getSignUpEmail?.errors?.['email']) {
      toast.error('Invalid email format');
      return;
    }

    if (this.getSignUpPassword?.errors?.['required']) {
      toast.error('Password is required');
      return;
    }

    if (this.getSignUpPassword?.errors?.['minlength']) {
      toast.error('Min length 6');
      return;
    }

    this.signupData = this.signUpForm.value;

    this.apiService.SignUpApi(this.signupData).subscribe({
      next: (response: CreateUserResponseDTO) => {
        if (response) {
          this.userDetails = response;
          console.dir(this.userDetails);
        }
      },
      error: (error: Error) => {
        console.error('SignUp failed: ' + error.message);
        toast.error('Signup Un successful');
      },
      complete: () => {
        toast.success('Signup Successfully');
      },
    });
    console.log('Signup Data:', this.signUpForm.value);
    this.signUpForm.reset();
  }

  toggleMode() {
    this.isLoginMode.set(!this.isLoginMode());
  }
}