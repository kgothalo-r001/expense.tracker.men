import { Component, OnInit } from '@angular/core';
import { DefaultRouteHandler } from '@expense-tracker/business';

@Component({
  selector: 'app-default-redirect',
  standalone: true,
  template: `
    <div class="loading-container">
      <div class="loading-spinner"></div>
      <p>Checking authentication...</p>
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100vh;
      background-color: var(--background-color);
      color: var(--text-color);
    }
    
    .loading-spinner {
      width: 40px;
      height: 40px;
      border: 4px solid var(--border-color);
      border-top: 4px solid var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin-bottom: 1rem;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class DefaultRedirectComponent implements OnInit {
  constructor(private defaultRouteHandler: DefaultRouteHandler) {}

  ngOnInit(): void {
    this.defaultRouteHandler.handleDefaultRoute();
  }
}
