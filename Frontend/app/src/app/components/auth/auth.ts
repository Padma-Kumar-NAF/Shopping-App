import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth.html',
  styleUrls: ['./auth.css'],
})
export class Auth {
  isLoginMode = true;

  loginData = {
    email: '',
    password: '',
  };

  signupData = {
    name: '',
    email: '',
    password: '',
  };

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
  }

  async onLogin() {
    const { email, password } = this.loginData;
    if (!email?.trim()) {
      toast.error('Email is required');
      return;
    }

    if (!password?.trim()) {
      toast.error('Password is required');
      return;
    }

    toast.success("Login Successfully")
    console.log('Login Data:', this.loginData);
  }

  async onSignup() {
    const { email, password, name } = this.signupData;

    if (!name?.trim()) {
      toast.error('Name is required');
      return;
    }

    if (!email?.trim()) {
      toast.error('Email is required');
      return;
    }

    if (!password?.trim()) {
      toast.error('Password is required');
      return;
    }

    toast.success("Signup Successfully")
    console.log('Signup Data:', this.signupData);
  }
}
