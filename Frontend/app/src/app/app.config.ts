import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideAppInitializer,
  inject,
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptors } from './interceptors/interceptor';
import { AuthStateService } from './services/auth-state.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),

    // withComponentInputBinding allows route params to bind directly to @Input()
    provideRouter(routes, withComponentInputBinding()),

    provideHttpClient(withInterceptors([authInterceptors])),

    // provideAppInitializer ensures user is restored from localStorage
    // BEFORE the router evaluates any guards.
    provideAppInitializer(() => {
      const authState = inject(AuthStateService);
      authState.loadUserFromStorage(); // synchronous — signal is set before guards run
    }),
  ],
};

// import {
//   ApplicationConfig,
//   provideBrowserGlobalErrorListeners,
//   provideAppInitializer,
//   inject,
// } from '@angular/core';

// import { provideRouter } from '@angular/router';
// import { routes } from './app.routes';

// import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
// import { provideHttpClient, withInterceptors } from '@angular/common/http';

// import { authInterceptors } from './interceptors/interceptor';
// import { AuthStateService } from './services/auth-state.service';

// export const appConfig: ApplicationConfig = {
//   providers: [
//     provideBrowserGlobalErrorListeners(),
//     provideRouter(routes),
//     provideClientHydration(withEventReplay()),
//     provideHttpClient(withInterceptors([authInterceptors])),
//     provideAppInitializer(() => {
//       const authState = inject(AuthStateService);
//       authState.loadUserFromStorage();
//       return Promise.resolve();
//     }),
//   ],
// };
