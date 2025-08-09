import { Component, OnInit, AfterViewInit, Input, ViewChild, ElementRef, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, ChartConfiguration, ChartType, registerables, TooltipItem } from 'chart.js';
import { Client } from '../../../../../auto/autoexpensetrackerclient';
import { Subject, takeUntil } from 'rxjs';

Chart.register(...registerables);

export interface BudgetProjectionData {
  month: string;
  projectedIncome: number;
  projectedExpenses: number;
  actualIncome: number;
  actualExpenses: number;
  budgetRemaining: number;
}

@Component({
  selector: 'app-budget-projection',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './budget-projection.component.html',
  styleUrls: ['./budget-projection.component.less']
})
export class BudgetProjectionComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() height: string = '400px';
  @ViewChild('chartCanvas', { static: false }) chartCanvas!: ElementRef<HTMLCanvasElement>;

  private chart: Chart | null = null;
  private destroy$ = new Subject<void>();
  
  isLoading = true;
  hasError = false;
  errorMessage = '';
  budgetData: any = null;
  selectedPeriod: 'weekly' | 'monthly' | 'yearly' = 'monthly';

  constructor(private client: Client, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadChartData();
  }

  ngAfterViewInit() {
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.destroyChart();
  }

  private loadChartData() {
    this.isLoading = true;
    this.hasError = false;
    
    this.client.getBudgetProjection()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: any) => {
          this.budgetData = data;
          this.isLoading = false;
          this.cdr.detectChanges();
          setTimeout(() => {
            if (this.chartCanvas?.nativeElement) {
              this.createChart(data);
            }
          }, 0);
        },
        error: (error: any) => {
          console.error('Failed to load budget projection:', error);
          this.hasError = true;
          this.errorMessage = 'Failed to load budget projection data';
          this.isLoading = false;
        }
      });
  }

  setPeriod(period: 'weekly' | 'monthly' | 'yearly') {
    this.selectedPeriod = period;
    if (this.budgetData && this.chartCanvas?.nativeElement) {
      this.createChart(this.budgetData);
    }
  }

  private createChart(data: any) {
    this.destroyChart();

    if (!this.chartCanvas?.nativeElement) {
      console.warn('Chart canvas not available yet');
      return;
    }

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    let months: string[], projectedExpenses: number[], recommendedBudget: number[];

    if (this.selectedPeriod === 'weekly') {
      months = ['Week 1', 'Week 2', 'Week 3', 'Week 4'];
      const weeklyExpense = data.projectedMonthlyExpenses / 4;
      const weeklySavings = data.recommendedMonthlySavings / 4;
      projectedExpenses = [weeklyExpense, weeklyExpense, weeklyExpense, weeklyExpense];
      recommendedBudget = [weeklySavings, weeklySavings, weeklySavings, weeklySavings];
    } else if (this.selectedPeriod === 'yearly') {
      months = ['This Year', 'Next Year'];
      projectedExpenses = [data.projectedYearlyExpenses, data.projectedYearlyExpenses * 1.05];
      recommendedBudget = [data.recommendedMonthlySavings * 12, data.recommendedMonthlySavings * 12 * 1.05];
    } else {
      const categories = data.categoryProjections?.slice(0, 6) || [];
      months = categories.map((cat: any) => cat.categoryName);
      projectedExpenses = categories.map((cat: any) => cat.averageMonthlySpending);
      recommendedBudget = categories.map((cat: any) => cat.recommendedBudget);
    }

    const config: ChartConfiguration = {
      type: 'bar' as ChartType,
      data: {
        labels: months,
        datasets: [
          {
            label: 'Projected Expenses',
            data: projectedExpenses,
            backgroundColor: 'rgba(239, 68, 68, 0.3)',
            borderColor: 'rgb(239, 68, 68)',
            borderWidth: 2,
            type: 'line',
            tension: 0.4,
            yAxisID: 'y'
          },
          {
            label: 'Recommended Budget',
            data: recommendedBudget,
            backgroundColor: 'rgba(34, 197, 94, 0.8)',
            borderColor: 'rgb(34, 197, 94)',
            borderWidth: 1,
            yAxisID: 'y'
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          intersect: false,
          mode: 'index'
        },
        plugins: {
          legend: {
            position: 'top',
            labels: {
              usePointStyle: true,
              padding: 15,
              font: {
                family: 'Segoe UI, Tahoma, Geneva, Verdana, sans-serif',
                size: 12
              }
            }
          },
          tooltip: {
            backgroundColor: 'rgba(31, 41, 55, 0.9)',
            titleColor: '#f9fafb',
            bodyColor: '#d1d5db',
            borderColor: '#374151',
            borderWidth: 1,
            callbacks: {
              label: (context: TooltipItem<'bar' | 'line'>) => {
                const value = new Intl.NumberFormat('en-ZA', {
                  style: 'currency',
                  currency: 'ZAR'
                }).format(context.parsed.y);
                return `${context.dataset.label}: ${value}`;
              }
            }
          }
        },
        scales: {
          x: {
            grid: {
              color: 'rgba(156, 163, 175, 0.1)'
            },
            ticks: {
              color: '#9ca3af',
              font: {
                family: 'Segoe UI, Tahoma, Geneva, Verdana, sans-serif'
              }
            }
          },
          y: {
            type: 'linear',
            display: true,
            position: 'left',
            grid: {
              color: 'rgba(156, 163, 175, 0.1)'
            },
            ticks: {
              color: '#9ca3af',
              font: {
                family: 'Segoe UI, Tahoma, Geneva, Verdana, sans-serif'
              },
              callback: (value: string | number) => {
                return new Intl.NumberFormat('en-ZA', {
                  style: 'currency',
                  currency: 'ZAR',
                  notation: 'compact'
                }).format(value as number);
              }
            }
          }
        }
      }
    };

    this.chart = new Chart(ctx, config);
  }

  private destroyChart() {
    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }
  }

  refreshChart() {
    this.loadChartData();
  }

  getBudgetHealth(): string {
    if (!this.budgetData) return 'unknown';
    
    const healthScore = this.budgetData.healthScore || 0;
    if (healthScore >= 80) return 'excellent';
    if (healthScore >= 60) return 'good';
    if (healthScore >= 40) return 'fair';
    return 'poor';
  }

  getBudgetHealthColor(): string {
    const health = this.getBudgetHealth();
    switch (health) {
      case 'excellent': return '#22c55e';
      case 'good': return '#3b82f6';
      case 'fair': return '#f59e0b';
      case 'poor': return '#ef4444';
      default: return '#6b7280';
    }
  }

  getFormattedAmount(amount: number): string {
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR'
    }).format(amount);
  }
}
