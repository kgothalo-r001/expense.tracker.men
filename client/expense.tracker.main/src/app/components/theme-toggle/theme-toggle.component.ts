import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button 
      class="theme-toggle btn btn-icon" 
      (click)="toggleTheme()"
      [attr.aria-label]="ariaLabel()"
      [title]="tooltipText()">
      <svg 
        class="theme-icon" 
        [class.sun-icon]="!isDarkMode()"
        [class.moon-icon]="isDarkMode()"
        width="20" 
        height="20" 
        viewBox="0 0 24 24" 
        fill="none" 
        stroke="currentColor" 
        stroke-width="2" 
        stroke-linecap="round" 
        stroke-linejoin="round">
        
        <!-- Sun Icon -->
        <g *ngIf="!isDarkMode()">
          <circle cx="12" cy="12" r="5"/>
          <path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/>
        </g>
        
        <!-- Moon Icon -->
        <g *ngIf="isDarkMode()">
          <path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/>
        </g>
      </svg>
      <span class="theme-text">{{ buttonText() }}</span>
    </button>
  `,
  styles: [`
    .theme-toggle {
      background-color: var(--color-surface-secondary);
      border: 1px solid var(--color-border);
      color: var(--color-text-primary);
      padding: var(--spacing-sm) var(--spacing-md);
      border-radius: var(--radius-lg);
      display: inline-flex;
      align-items: center;
      gap: var(--spacing-sm);
      transition: all var(--transition-base);
      cursor: pointer;
      
      &:hover {
        background-color: var(--color-surface-tertiary);
        border-color: var(--color-border-secondary);
        transform: translateY(-1px);
        box-shadow: var(--shadow-sm);
      }
      
      &:active {
        transform: translateY(0);
      }
    }
    
    .theme-icon {
      transition: transform var(--transition-base);
      flex-shrink: 0;
      
      &.sun-icon {
        color: var(--warning-500);
      }
      
      &.moon-icon {
        color: var(--info-400);
      }
    }
    
    .theme-text {
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-medium);
      white-space: nowrap;
      
      @media (max-width: 640px) {
        display: none;
      }
    }
    
    /* Hover animation for icons */
    .theme-toggle:hover .theme-icon {
      transform: rotate(15deg) scale(1.1);
    }
    
    /* Focus styles for accessibility */
    .theme-toggle:focus {
      outline: 2px solid var(--color-primary);
      outline-offset: 2px;
    }
  `]
})
export class ThemeToggleComponent {
  private themeService = inject(ThemeService);
  
  // Computed properties for reactive UI
  readonly isDarkMode = computed(() => this.themeService.isDarkMode());
  readonly currentTheme = computed(() => this.themeService.getTheme());
  
  readonly buttonText = computed(() => 
    this.isDarkMode() ? 'Light Mode' : 'Dark Mode'
  );
  
  readonly ariaLabel = computed(() => 
    `Switch to ${this.isDarkMode() ? 'light' : 'dark'} mode`
  );
  
  readonly tooltipText = computed(() => 
    `Switch to ${this.isDarkMode() ? 'light' : 'dark'} mode`
  );
  
  toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
