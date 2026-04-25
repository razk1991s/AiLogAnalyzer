import { Pipe, PipeTransform } from '@angular/core';
import { Severity } from '../models/log.model';

@Pipe({ name: 'severityClass', standalone: true })
export class SeverityClassPipe implements PipeTransform {
  transform(severity: Severity): string {
    const map: Record<Severity, string> = {
      critical: 'badge badge--critical',
      high: 'badge badge--high',
      medium: 'badge badge--medium',
      low: 'badge badge--low',
      info: 'badge badge--info'
    };
    return map[severity] ?? 'badge badge--info';
  }
}

@Pipe({ name: 'severityIcon', standalone: true })
export class SeverityIconPipe implements PipeTransform {
  transform(severity: Severity): string {
    const map: Record<Severity, string> = {
      critical: '🔴',
      high: '🟠',
      medium: '🟡',
      low: '🔵',
      info: '⚪'
    };
    return map[severity] ?? '⚪';
  }
}
