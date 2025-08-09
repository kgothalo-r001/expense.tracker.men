import { Component, OnInit, AfterViewInit, Input, ViewChild, ElementRef, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, ChartConfiguration, ChartType, registerables, TooltipItem, ScaleOptionsByType } from 'chart.js';
import { Client, MonthlySpending } from '../../../../../auto/autoexpensetrackerclient';
import { Subject, takeUntil } from 'rxjs';

Chart.register(...registerables);

@Component({
  selector: 'app-monthly-spending-trends',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './monthly-spending-trends.component.html',
  styleUrls: ['./monthly-spending-trends.component.less']
})
export class MonthlySpendingTrendsComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() monthsBack: number = 12;
  @Input() height: string = '400px';
  @ViewChild('chartCanvas', { static: false }) chartCanvas!: ElementRef<HTMLCanvasElement>;

  private chart: Chart | null = null;
  private destroy$ = new Subject<void>();
  
  isLoading = true;
  hasError = false;
  errorMessage = '';

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
    
    this.client.getMonthlySpendingTrends(this.monthsBack)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: MonthlySpending[]) => {
          this.isLoading = false;
          this.cdr.detectChanges();
          setTimeout(() => {
            if (this.chartCanvas?.nativeElement) {
              this.createChart(data);
            }
          }, 0);
        },
        error: (error: any) => {
          console.error('Failed to load monthly spending trends:', error);
          this.hasError = true;
          this.errorMessage = 'Failed to load spending trends data';
          this.isLoading = false;
        }
      });
  }

  private createChart(data: MonthlySpending[]) {
    this.destroyChart();

    if (!this.chartCanvas?.nativeElement) {
      console.warn('Chart canvas not available yet');
      return;
    }

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    // Process the data
    const labels = data.map(item => {
      const date = new Date((item.month || '') + '-01');
      return date.toLocaleDateString('en-ZA', { month: 'short', year: 'numeric' });
    });
    const expenseData = data.map(item => item.amount || 0);

    const config: ChartConfiguration = {
      type: 'line' as ChartType,
      data: {
        labels,
        datasets: [
          {
            label: 'Monthly Expenses',
            data: expenseData,
            borderColor: 'rgb(239, 68, 68)',
            backgroundColor: 'rgba(239, 68, 68, 0.1)',
            tension: 0.4,
            fill: true
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
              padding: 20,
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
              label: (context: TooltipItem<'line'>) => {
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
}
