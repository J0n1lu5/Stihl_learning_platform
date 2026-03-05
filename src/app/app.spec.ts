import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { PLATFORM_ID } from '@angular/core';
import { App } from './app';
import { ApiStatusService } from './core/api-status.service';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        {
          provide: ApiStatusService,
          useValue: {
            getHealth: () =>
              of({ status: 'ok', service: 'test-api', timestampUtc: '2026-01-01T00:00:00Z' }),
            getIntroModule: () =>
              of({
                id: 'm1',
                title: 'Testmodul',
                description: 'desc',
                questions: [
                  {
                    id: 'q1',
                    prompt: 'Frage?',
                    options: [
                      { id: 'a', text: 'A' },
                      { id: 'b', text: 'B' }
                    ]
                  }
                ]
              }),
            submitIntroQuiz: () => of(),
            getUserProgress: () =>
              of({
                userId: 'demo-user',
                attempts: 1,
                bestScorePercent: 100,
                lastScorePercent: 100,
                completedModules: 1,
                lastCompletedUtc: null
              })
          }
        },
        { provide: PLATFORM_ID, useValue: 'browser' }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render app title', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('STIHL Learning Platform');
  });
});
