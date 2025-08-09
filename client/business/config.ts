import { InjectionToken } from '@angular/core';
import { IClient } from './auto/autobusinessclient';

// Create injection token for the client
export const API_CLIENT = new InjectionToken<IClient>('API_CLIENT');
