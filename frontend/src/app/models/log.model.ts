export interface LogEntry {
  id: string;
  logText: string;
  issue: string;
  severity: Severity;
  explanation: string;
  solution: string;
  createdAt: string;
}

export type Severity = 'critical' | 'high' | 'medium' | 'low' | 'info';

export interface AnalyzeLogRequest {
  logText: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
