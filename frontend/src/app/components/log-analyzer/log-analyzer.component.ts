import { Component, inject, signal, OnInit } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { LogService } from "../../services/log.service";
import { LogEntry } from "../../models/log.model";
import { SeverityClassPipe, SeverityIconPipe } from "../../pipes/severity.pipe";

@Component({
  selector: "app-log-analyzer",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SeverityClassPipe,
    SeverityIconPipe,
  ],
  templateUrl: "./log-analyzer.component.html",
  styleUrls: ["./log-analyzer.component.scss"],
})
export class LogAnalyzerComponent implements OnInit {
  private readonly logService = inject(LogService);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    logText: ["", [Validators.required, Validators.minLength(10)]],
  });

  readonly isLoading = signal(false);
  readonly result = signal<LogEntry | null>(null);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.form.controls.logText.valueChanges.subscribe((value) => {});
  }

  get logTextControl() {
    return this.form.controls.logText;
  }

  onSubmit(): void {
    if (this.form.invalid || this.isLoading()) return;

    this.isLoading.set(true);
    this.result.set(null);
    this.errorMessage.set(null);

    this.logService
      .analyzeLog({ logText: this.form.value.logText! })
      .subscribe({
        next: (response) => {
          this.result.set(response);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.errorMessage.set(
            err?.error?.message ?? "Analysis failed. Please try again.",
          );
          this.isLoading.set(false);
        },
      });
  }

  clearResult(): void {
    this.result.set(null);
    this.form.reset();
  }

  readonly sampleLogs = [
    `[2024-01-15 14:23:45] ERROR System.NullReferenceException: Object reference not set to an instance of an object.
   at UserService.GetUserById(Int32 userId) in UserService.cs:line 42
   at UsersController.GetUser(Int32 id) in UsersController.cs:line 28`,
    `[2024-01-15 14:31:02] FATAL Unhandled exception. System.OutOfMemoryException: Insufficient memory to continue the execution of the program.
   at System.Collections.Generic.List\`1.set_Capacity(Int32 value)
   at ImageProcessor.ResizeImages(List images)`,
    `[2024-01-15 09:12:33] ERROR Npgsql.NpgsqlException (0x80004005): Exception while reading from stream
Connection refused (localhost:5432)
   at DatabaseContext.OnConfiguring`,
  ];

  useSampleLog(log: string): void {
    this.form.patchValue({ logText: log });
  }
}
