import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AnalyzeLogRequest, LogEntry, PagedResult } from '../models/log.model';

@Injectable({
  providedIn: 'root'
})
export class LogService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/logs`;

  analyzeLog(request: AnalyzeLogRequest): Observable<LogEntry> {
    return this.http.post<LogEntry>(`${this.apiUrl}/analyze`, request);
  }

  getLogs(page = 1, pageSize = 20): Observable<PagedResult<LogEntry>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<LogEntry>>(this.apiUrl, { params });
  }

  getLogById(id: string): Observable<LogEntry> {
    return this.http.get<LogEntry>(`${this.apiUrl}/${id}`);
  }

  deleteLog(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
