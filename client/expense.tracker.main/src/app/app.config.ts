import { ApplicationConfig, provideZoneChangeDetection, InjectionToken } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { Client, IClient, API_BASE_URL } from '../../auto/autoexpensetrackerclient';
import { ApiService } from '../services/api.service';
import { BUSINESS_PROVIDERS } from '../../../business';

import { routes } from './app.routes';

// Create injection token for the client
export const API_CLIENT = new InjectionToken<IClient>('API_CLIENT');

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    { 
      provide: API_CLIENT, 
      useClass: Client 

    },
    { 
      provide: API_BASE_URL, 
      useValue: 'https://localhost:49871' 
    },
    ApiService,
    ...BUSINESS_PROVIDERS
  ]
};
