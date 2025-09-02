import { Component, OnInit, AfterViewInit, Input, ViewChild, ElementRef, OnDestroy, ChangeDetectorRef, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, ChartConfiguration, ChartType, registerables, TooltipItem } from 'chart.js';
import { CategoryTrend } from '../../../../../auto/autoexpensetrackerclient';
import { CategoryService } from '../../../../../../business';
import { Subject, takeUntil } from 'rxjs';

Chart.register(...registerables);

export interface CategoryTrendData {
  category: string;
  amount: number;
  percentage: number;
  color?: string;
}

@Component({
  selector: 'app-category-trends',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './category-trends.component.html',
  styleUrls: ['./category-trends.component.less']
})
export class CategoryTrendsComponent implements OnInit, AfterViewInit, OnDestroy, OnChanges {
  @Input() chartType: 'pie' | 'doughnut' = 'doughnut';
  @Input() height: string = '400px';
  @Input() data: CategoryTrend[] | null = null;
  @ViewChild('chartCanvas', { static: false }) chartCanvas!: ElementRef<HTMLCanvasElement>;

  private chart: Chart | null = null;
  private destroy$ = new Subject<void>();
  
  isLoading = true;
  hasError = false;
  errorMessage = '';
  categoryData: CategoryTrendData[] = [];

  constructor(
    private categoryService: CategoryService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (this.data) {
      this.loadChartData();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data'] && changes['data'].currentValue) {
      this.loadChartData();
    }
  }

  ngAfterViewInit() {
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.destroyChart();
  }

  private loadChartData() {
    if (!this.data || this.data.length === 0) {
      this.isLoading = false;
      this.hasError = true;
      this.errorMessage = 'No category trends data available';
      return;
    }

    this.isLoading = true;
    this.hasError = false;
    
    this.categoryService.getCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (categories) => {
          this.processData(this.data!, categories);
          this.isLoading = false;
          this.cdr.detectChanges();
          setTimeout(() => {
            if (this.chartCanvas?.nativeElement) {
              this.createChart();
            }
          }, 0);
        },
        error: (error: any) => {
          console.error('Failed to load categories:', error);
          this.hasError = true;
          this.errorMessage = 'Failed to load categories data';
          this.isLoading = false;
        }
      });
  }

  private processData(data: CategoryTrend[], categories: any[]) {
    const filteredData = data.filter(item => 
      (item.currentMonthAmount || 0) > 0 || (item.previousMonthAmount || 0) > 0
    );
    
    const hasCurrentMonthData = filteredData.some(item => (item.currentMonthAmount || 0) > 0);
    const useCurrentMonth = hasCurrentMonthData;
    
    const totalAmount = filteredData.reduce((sum, item) => {
      const amount = useCurrentMonth ? (item.currentMonthAmount || 0) : (item.previousMonthAmount || 0);
      return sum + amount;
    }, 0);
    
    const categoryColorMap = new Map<string, string>();
    categories.forEach(cat => {
      categoryColorMap.set(cat.name, cat.color);
    });
    
    this.categoryData = filteredData.map((item, index) => {
      const amount = useCurrentMonth ? (item.currentMonthAmount || 0) : (item.previousMonthAmount || 0);
      return {
        category: item.categoryName || 'Unknown',
        amount: amount,
        percentage: totalAmount > 0 ? (amount / totalAmount) * 100 : 0,
        color: categoryColorMap.get(item.categoryName || '') || this.getDefaultColor(index)
      };
    }).filter(item => item.amount > 0);
  }
  
  private getDefaultColor(index: number): string {
    const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#84cc16', '#f97316'];
    return colors[index % colors.length];
  }

  private createChart() {
    this.destroyChart();

    if (!this.chartCanvas?.nativeElement) {
      return;
    }
    
    if (!this.categoryData || this.categoryData.length === 0) {
      this.hasError = true;
      this.errorMessage = 'No category data available to display';
      return;
    }

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    const labels = this.categoryData.map(item => item.category);
    const data = this.categoryData.map(item => item.amount);
    const backgroundColors = this.categoryData.map(item => item.color || '#3b82f6');
    const borderColors = this.categoryData.map(item => item.color || '#3b82f6');

    const config: ChartConfiguration = {
      type: this.chartType as ChartType,
      data: {
        labels,
        datasets: [{
          data,
          backgroundColor: backgroundColors,
          borderColor: borderColors,
          borderWidth: 2,
          hoverBorderWidth: 3
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'right',
            align: 'center',
            labels: {
              usePointStyle: true,
              padding: 15,
              font: {
                family: 'Segoe UI, Tahoma, Geneva, Verdana, sans-serif',
                size: 12
              },
              generateLabels: (chart: Chart) => {
                const dataset = chart.data.datasets[0];
                return chart.data.labels?.map((label: any, index: number) => {
                  const amount = dataset.data[index] as number;
                  const percentage = this.categoryData[index]?.percentage || 0;
                  const color = this.categoryData[index]?.color || '#3b82f6';
                  return {
                    text: `${label} (${percentage.toFixed(1)}%)`,
                    fillStyle: color,
                    strokeStyle: color,
                    lineWidth: 2,
                    pointStyle: 'circle',
                    hidden: false,
                    index
                  };
                }) || [];
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
              label: (context: TooltipItem<typeof this.chartType>) => {
                const amount = context.parsed as number;
                const percentage = this.categoryData[context.dataIndex]?.percentage || 0;
                const formattedAmount = new Intl.NumberFormat('en-ZA', {
                  style: 'currency',
                  currency: 'ZAR'
                }).format(amount);
                return `${context.label}: ${formattedAmount} (${percentage.toFixed(1)}%)`;
              }
            }
          }
        },
        layout: {
          padding: {
            left: 10,
            right: 10,
            top: 10,
            bottom: 10
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

  toggleChartType() {
    this.chartType = this.chartType === 'pie' ? 'doughnut' : 'pie';
    if (!this.isLoading && !this.hasError) {
      this.createChart();
    }
  }

  getTotalAmount(): number {
    return this.categoryData.reduce((sum, item) => sum + item.amount, 0);
  }

  getFormattedAmount(amount: number): string {
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR'
    }).format(amount);
  }

  trackByCategory(index: number, item: CategoryTrendData): string {
    return item.category;
  }
}
