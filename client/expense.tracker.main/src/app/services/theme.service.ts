import { Injectable, signal, effect } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'expense-tracker-theme';
  
  // Reactive signal for current theme
  public readonly currentTheme = signal<Theme>(this.getInitialTheme());
  
  constructor() {
    // Effect to apply theme changes to DOM
    effect(() => {
      this.applyTheme(this.currentTheme());
    });
  }
  
  /**
   * Toggle between light and dark theme
   */
  toggleTheme(): void {
    const newTheme: Theme = this.currentTheme() === 'light' ? 'dark' : 'light';
    this.setTheme(newTheme);
  }
  
  /**
   * Set specific theme
   */
  setTheme(theme: Theme): void {
    this.currentTheme.set(theme);
    this.saveThemeToStorage(theme);
  }
  
  /**
   * Get the current theme value
   */
  getTheme(): Theme {
    return this.currentTheme();
  }
  
  /**
   * Check if current theme is dark
   */
  isDarkMode(): boolean {
    return this.currentTheme() === 'dark';
  }
  
  /**
   * Apply theme to DOM
   */
  private applyTheme(theme: Theme): void {
    const htmlElement = document.documentElement;
    
    if (theme === 'dark') {
      htmlElement.setAttribute('data-theme', 'dark');
    } else {
      htmlElement.removeAttribute('data-theme');
    }
  }
  
  /**
   * Get initial theme from storage or system preference
   */
  private getInitialTheme(): Theme {
    // First check localStorage
    const savedTheme = this.getThemeFromStorage();
    if (savedTheme) {
      return savedTheme;
    }
    
    // Fallback to system preference
    return this.getSystemThemePreference();
  }
  
  /**
   * Get theme from localStorage
   */
  private getThemeFromStorage(): Theme | null {
    try {
      const saved = localStorage.getItem(this.THEME_KEY);
      return saved === 'dark' ? 'dark' : saved === 'light' ? 'light' : null;
    } catch {
      return null;
    }
  }
  
  /**
   * Save theme to localStorage
   */
  private saveThemeToStorage(theme: Theme): void {
    try {
      localStorage.setItem(this.THEME_KEY, theme);
    } catch {
      // Handle storage errors silently
    }
  }
  
  /**
   * Get system theme preference
   */
  private getSystemThemePreference(): Theme {
    if (typeof window === 'undefined') {
      return 'light';
    }
    
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
  
  /**
   * Listen to system theme changes
   */
  initSystemThemeListener(): void {
    if (typeof window === 'undefined') {
      return;
    }
    
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    const handleChange = (e: MediaQueryListEvent) => {
      // Only auto-switch if user hasn't manually set a preference
      if (!this.getThemeFromStorage()) {
        this.setTheme(e.matches ? 'dark' : 'light');
      }
    };
    
    // Modern browsers
    if (mediaQuery.addEventListener) {
      mediaQuery.addEventListener('change', handleChange);
    } else {
      // Legacy browsers
      mediaQuery.addListener(handleChange);
    }
  }
}
