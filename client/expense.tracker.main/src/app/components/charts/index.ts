// Chart Components
export * from './monthly-spending-trends/monthly-spending-trends.component';
export * from './category-trends/category-trends.component';
export * from './budget-projection/budget-projection.component';

// Chart Component Array for easy module registration
import { MonthlySpendingTrendsComponent } from './monthly-spending-trends/monthly-spending-trends.component';
import { CategoryTrendsComponent } from './category-trends/category-trends.component';
import { BudgetProjectionComponent } from './budget-projection/budget-projection.component';

export const CHART_COMPONENTS = [
  MonthlySpendingTrendsComponent,
  CategoryTrendsComponent,
  BudgetProjectionComponent
];
