import { Component, inject, signal, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { LogService } from "../../services/log.service";
import { LogEntry } from "../../models/log.model";
import { SeverityClassPipe, SeverityIconPipe } from "../../pipes/severity.pipe";

@Component({
  selector: "app-log-history",
  standalone: true,
  imports: [CommonModule, RouterModule, SeverityClassPipe, SeverityIconPipe],
  templateUrl: "./log-history.component.html",
  styleUrls: ["./log-history.component.scss"],
})
export class LogHistoryComponent implements OnInit {
  private readonly logService = inject(LogService);

  readonly logs = signal<LogEntry[]>([]);
  readonly isLoading = signal(false);
  readonly totalCount = signal(0);
  readonly currentPage = signal(1);
  readonly pageSize = 20;
  readonly errorMessage = signal<string | null>(null);
  readonly expandedId = signal<string | null>(null);

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.logService.getLogs(this.currentPage(), this.pageSize).subscribe({
      next: (result) => {
        this.logs.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set("Failed to load logs.");
        this.isLoading.set(false);
      },
    });
  }

  toggleExpand(id: string): void {
    this.expandedId.update((current) => (current === id ? null : id));
  }

  isExpanded(id: string): boolean {
    return this.expandedId() === id;
  }

  deleteLog(id: string, event: Event): void {
    event.stopPropagation();
    if (!confirm("Delete this log entry?")) return;

    this.logService.deleteLog(id).subscribe({
      next: () => {
        this.logs.update((logs) => logs.filter((l) => l.id !== id));
        if (this.expandedId() === id) this.expandedId.set(null);
      },
      error: () => this.errorMessage.set("Failed to delete log."),
    });
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.expandedId.set(null);
    this.currentPage.set(page);
    this.loadLogs();
  }

  truncate(text: string, max = 120): string {
    return text.length > max ? text.slice(0, max) + "..." : text;
  }
}
