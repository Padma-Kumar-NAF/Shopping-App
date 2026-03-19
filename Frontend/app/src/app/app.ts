import { Component, signal ,inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxSonnerToaster } from 'ngx-sonner';
import { NavbarComponent } from './components/shared/navbar/navbar';
import { LoaderService } from './services/loading.service';
import { Spinner } from "./components/spinner/spinner";
import { AdminDashboard } from './components/adminComponents/admin-dashboard/admin-dashboard';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    NgxSonnerToaster,
    NavbarComponent,
    Spinner,
    AdminDashboard
],  
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('app');
  loader = inject(LoaderService);
}
