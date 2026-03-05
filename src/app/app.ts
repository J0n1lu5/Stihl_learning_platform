import { HttpErrorResponse } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Component, OnInit, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import {
  ApiHealth,
  ApiStatusService,
  LearningModule,
  QuizAnswerResult,
  QuizSubmissionResponse,
  UserProgress
} from './core/api-status.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = 'STIHL Learning Platform';
  protected readonly userId = 'demo-user';

  protected readonly health = signal<ApiHealth | null>(null);
  protected readonly healthError = signal<string | null>(null);

  protected readonly learningModule = signal<LearningModule | null>(null);
  protected readonly moduleLoading = signal(true);
  protected readonly moduleError = signal<string | null>(null);

  protected readonly selectedAnswers = signal<Record<string, string>>({});
  protected readonly submitLoading = signal(false);
  protected readonly submitError = signal<string | null>(null);
  protected readonly submissionResult = signal<QuizSubmissionResponse | null>(null);
  protected readonly progress = signal<UserProgress | null>(null);

  protected readonly allQuestionsAnswered = computed(() => {
    const module = this.learningModule();
    if (!module) {
      return false;
    }

    const answers = this.selectedAnswers();
    return module.questions.every((question) => Boolean(answers[question.id]));
  });

  private readonly apiStatus = inject(ApiStatusService);
  private readonly platformId = inject(PLATFORM_ID);

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      this.moduleLoading.set(false);
      return;
    }

    this.loadHealth();
    this.loadModule();
    this.loadProgress();
  }

  protected selectAnswer(questionId: string, optionId: string): void {
    const next = { ...this.selectedAnswers() };
    next[questionId] = optionId;
    this.selectedAnswers.set(next);
  }

  protected isSelected(questionId: string, optionId: string): boolean {
    return this.selectedAnswers()[questionId] === optionId;
  }

  protected submitQuiz(): void {
    const module = this.learningModule();
    if (!module || !this.allQuestionsAnswered() || this.submitLoading()) {
      return;
    }

    this.submitLoading.set(true);
    this.submitError.set(null);

    this.apiStatus
      .submitIntroQuiz({
        userId: this.userId,
        answers: this.selectedAnswers()
      })
      .subscribe({
        next: (result) => {
          this.submissionResult.set(result);
          this.progress.set(result.progress);
          this.submitLoading.set(false);
        },
        error: (err: unknown) => {
          this.submitError.set(this.formatError('Quiz konnte nicht gesendet werden', err));
          this.submitLoading.set(false);
        }
      });
  }

  protected resetQuiz(): void {
    this.selectedAnswers.set({});
    this.submissionResult.set(null);
    this.submitError.set(null);
  }

  protected questionResult(questionId: string): QuizAnswerResult | null {
    const result = this.submissionResult();
    if (!result) {
      return null;
    }

    return result.results.find((item) => item.questionId === questionId) ?? null;
  }

  private loadHealth(): void {
    this.apiStatus.getHealth().subscribe({
      next: (value) => this.health.set(value),
      error: (err: unknown) => this.healthError.set(this.formatError('Backend nicht erreichbar', err))
    });
  }

  private loadModule(): void {
    this.moduleLoading.set(true);
    this.moduleError.set(null);

    this.apiStatus.getIntroModule().subscribe({
      next: (module) => {
        this.learningModule.set(module);
        this.moduleLoading.set(false);
      },
      error: (err: unknown) => {
        this.moduleError.set(this.formatError('Lernmodul konnte nicht geladen werden', err));
        this.moduleLoading.set(false);
      }
    });
  }

  private loadProgress(): void {
    this.apiStatus.getUserProgress(this.userId).subscribe({
      next: (progress) => this.progress.set(progress),
      error: () => {
        // Progress is optional for first start.
      }
    });
  }

  private formatError(prefix: string, err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      return `${prefix} (${err.status} ${err.statusText})`;
    }

    return `${prefix} (Unbekannter Fehler)`;
  }
}
