import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { DashboardSummary } from '@abstractions/models';
import { DashboardService } from '@business/services';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.less'
})
export class DashboardComponent implements OnInit {
  dashboardSummary$!: Observable<DashboardSummary>;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.dashboardSummary$ = this.dashboardService.getDashboardSummary();
  }

  onDateRangeChange(startDate: Date, endDate: Date): void {
    this.dashboardSummary$ = this.dashboardService.getDashboardSummary(startDate, endDate);
  }
}
