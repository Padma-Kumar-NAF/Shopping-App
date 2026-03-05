import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Auth} from './components/auth/auth'
import { Home } from './components/home/home';
import { Profile } from './components/profile/profile';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Auth,Home,Profile],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('app');
}
