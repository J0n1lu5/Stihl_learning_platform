import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type ApiHealth = {
  status: string;
  service: string;
  timestampUtc: string;
};

export type QuestionOption = {
  id: string;
  text: string;
};

export type QuizQuestion = {
  id: string;
  prompt: string;
  options: QuestionOption[];
};

export type LearningModule = {
  id: string;
  title: string;
  description: string;
  questions: QuizQuestion[];
};

export type SubmitQuizRequest = {
  userId: string;
  answers: Record<string, string>;
};

export type QuizAnswerResult = {
  questionId: string;
  selectedOptionId: string | null;
  correctOptionId: string;
  isCorrect: boolean;
  explanation: string;
};

export type UserProgress = {
  userId: string;
  attempts: number;
  bestScorePercent: number;
  lastScorePercent: number;
  completedModules: number;
  lastCompletedUtc: string | null;
};

export type QuizSubmissionResponse = {
  moduleId: string;
  totalQuestions: number;
  correctAnswers: number;
  scorePercent: number;
  passed: boolean;
  results: QuizAnswerResult[];
  progress: UserProgress;
};

@Injectable({
  providedIn: 'root'
})
export class ApiStatusService {
  private readonly http = inject(HttpClient);

  getHealth(): Observable<ApiHealth> {
    return this.http.get<ApiHealth>('/api/health');
  }

  getIntroModule(): Observable<LearningModule> {
    return this.http.get<LearningModule>('/api/modules/intro');
  }

  submitIntroQuiz(payload: SubmitQuizRequest): Observable<QuizSubmissionResponse> {
    return this.http.post<QuizSubmissionResponse>('/api/modules/intro/submit', payload);
  }

  getUserProgress(userId: string): Observable<UserProgress> {
    return this.http.get<UserProgress>(`/api/progress/${encodeURIComponent(userId)}`);
  }
}
