import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Auth} from './components/auth/auth'
import { Home } from './components/home/home';
import { Profile } from './components/profileComponents/profile/profile';
import { NgxSonnerToaster  } from 'ngx-sonner';
import { Spinner } from './components/spinner/spinner';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Auth,Home,Profile,NgxSonnerToaster,Spinner],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('app');
}
