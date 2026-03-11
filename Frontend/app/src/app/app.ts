import { Component, Signal, signal  } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Auth} from './components/auth/auth'
import { Profile } from './components/profileComponents/profile/profile';
import { NgxSonnerToaster  } from 'ngx-sonner';
import { Spinner } from './components/spinner/spinner';
import { HomeComponent } from './components/homeComponents/home/home';
import { Game } from './game/game';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Auth,Profile,NgxSonnerToaster,Spinner,HomeComponent,Game],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App {
  protected readonly title = signal('app');
  count = signal(0);

  increaseCount(){
    this.count.update(c => c + 1)
  }
  resetCount(){
    this.count.set(0)
  }
}
