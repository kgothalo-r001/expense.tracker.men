import { ApplicationConfig, provideZoneChangeDetection, InjectionToken } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { Client, IClient, API_BASE_URL } from '../../auto/autoexpensetrackerclient';
import { transactionReducer } from './store';
import { TransactionEffects } from './store';
import { categoryReducer } from './store/category/category.reducer';
import { CategoryEffects } from './store/category/category.effects';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { routes } from './app.routes';

// Create injection token for the client
export const API_CLIENT = new InjectionToken<IClient>('API_CLIENT');

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideStore({
      transactions: transactionReducer,
      categories: categoryReducer
    }),
    provideEffects([TransactionEffects, CategoryEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: false,
      autoPause: true
    }),
    { 
      provide: API_CLIENT, 
      useClass: Client 
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
