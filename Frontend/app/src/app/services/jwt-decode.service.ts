import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

export class JwtDecodeService {

  decodeToken(token: string): any {
    try {
      return jwtDecode(token);
    } catch (error) {
      console.error('Invalid token');
      return null;
    }
  }
}