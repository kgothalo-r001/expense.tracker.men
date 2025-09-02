import { ApplicationConfig, provideZoneChangeDetection, InjectionToken } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { Client, IClient, API_BASE_URL } from '../../auto/autoexpensetrackerclient';
import { 
  transactionReducer, 
  TransactionEffects, 
  categoryReducer, 
  CategoryEffects,
  dashboardReducer,
  DashboardEffects,
  AuthInterceptor, 
  API_CLIENT as BUSINESS_API_CLIENT 
} from '../../../business';
import { routes } from './app.routes';

export const API_CLIENT = new InjectionToken<IClient>('API_CLIENT');

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideStore({
      transactions: transactionReducer,
      categories: categoryReducer,
      dashboard: dashboardReducer
    }),
    provideEffects([TransactionEffects, CategoryEffects, DashboardEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: false,
      autoPause: true
    }),
    { 
      provide: BUSINESS_API_CLIENT, 
      useClass: Client 
    },
    {
      provide: API_CLIENT,
      useExisting: BUSINESS_API_CLIENT
    },
    {
      provide: API_BASE_URL, 
      useValue: 'https://localhost:49871'
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
};
