import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/log-analyzer/log-analyzer.component').then(m => m.LogAnalyzerComponent)
  },
  {
    path: 'history',
    loadComponent: () =>
      import('./components/log-history/log-history.component').then(m => m.LogHistoryComponent)
  },
  { path: '**', redirectTo: '' }
];
