// Re-export all business logic
export * from './services';
export * from './state';
export * from './utils';
export * from './constants';
export * from './auto/autobusinessclient';

import { InjectionToken, Provider } from '@angular/core';
import { IClient, Client, API_BASE_URL } from './auto/autobusinessclient';

export const ICLIENT_TOKEN = new InjectionToken<IClient>('BusinessIClient');

export const BUSINESS_PROVIDERS: Provider[] = [
  {
    provide: ICLIENT_TOKEN,
    useClass: Client
  },
  {
    provide: API_BASE_URL,
    useValue: 'https://localhost:49871'
  }
];
