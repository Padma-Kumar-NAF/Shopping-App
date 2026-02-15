import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth.html',
  styleUrls: ['./auth.css']
})
export class Auth {

  isLoginMode = true;

  loginData = {
    email: '',
    password: ''
  };

  signupData = {
    name: '',
    email: '',
    password: ''
  };

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
  }

  onLogin() {
    console.log('Login Data:', this.loginData);
  }

  onSignup() {
    console.log('Signup Data:', this.signupData);
  }
}
