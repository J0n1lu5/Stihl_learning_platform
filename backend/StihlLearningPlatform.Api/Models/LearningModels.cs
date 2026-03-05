public sealed record QuestionOption(string Id, string Text);

public sealed record QuizQuestion(
    string Id,
    string Prompt,
    IReadOnlyList<QuestionOption> Options,
    string CorrectOptionId,
    string Explanation);

public sealed record LearningModule(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<QuizQuestion> Questions);

public sealed record QuestionOptionDto(string Id, string Text);

public sealed record QuizQuestionDto(
    string Id,
    string Prompt,
    IReadOnlyList<QuestionOptionDto> Options);

public sealed record LearningModuleDto(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<QuizQuestionDto> Questions);

public sealed record SubmitQuizRequest(string UserId, Dictionary<string, string> Answers);

public sealed record QuizAnswerResult(
    string QuestionId,
    string? SelectedOptionId,
    string CorrectOptionId,
    bool IsCorrect,
    string Explanation);

public sealed record QuizSubmissionResponse(
    string ModuleId,
    int TotalQuestions,
    int CorrectAnswers,
    double ScorePercent,
    bool Passed,
    IReadOnlyList<QuizAnswerResult> Results,
    UserProgress Progress);

public sealed record UserProgress(
    string UserId,
    int Attempts,
    double BestScorePercent,
    double LastScorePercent,
    int CompletedModules,
    DateTime? LastCompletedUtc)
{
    public static UserProgress CreateEmpty(string userId) => new(userId, 0, 0, 0, 0, null);

    public static UserProgress CreateNew(string userId, double scorePercent, bool passed) => new(
        userId,
        Attempts: 1,
        BestScorePercent: scorePercent,
        LastScorePercent: scorePercent,
        CompletedModules: passed ? 1 : 0,
        LastCompletedUtc: passed ? DateTime.UtcNow : null);

    public UserProgress Update(double scorePercent, bool passed)
    {
        var completedModules = CompletedModules;
        var lastCompletedUtc = LastCompletedUtc;

        if (passed && scorePercent >= 70 && LastScorePercent < 70)
        {
            completedModules += 1;
            lastCompletedUtc = DateTime.UtcNow;
        }

        if (passed && LastCompletedUtc is null)
        {
            completedModules = Math.Max(1, completedModules);
            lastCompletedUtc = DateTime.UtcNow;
        }

        return this with
        {
            Attempts = Attempts + 1,
            BestScorePercent = Math.Max(BestScorePercent, scorePercent),
            LastScorePercent = scorePercent,
            CompletedModules = completedModules,
            LastCompletedUtc = lastCompletedUtc
        };
    }
}

public static class LearningModuleFactory
{
    public static LearningModule CreateIntroModule()
    {
        return new LearningModule(
            "intro-safety-basics",
            "Grundlagen: Sicherheit und Technik",
            "Drei Einstiegsfragen zu Sicherheitsregeln und Funktionsprinzipien.",
            new List<QuizQuestion>
            {
                new(
                    "q1",
                    "Was ist vor der Inbetriebnahme einer Motorsaege am wichtigsten?",
                    new List<QuestionOption>
                    {
                        new("q1o1", "Kette ölen und Kettenspannung pruefen"),
                        new("q1o2", "Direkt Vollgas geben"),
                        new("q1o3", "Nur den Tankstand kontrollieren")
                    },
                    "q1o1",
                    "Sichere Funktion und korrekte Spannung reduzieren Unfall- und Verschleissrisiko."),
                new(
                    "q2",
                    "Wozu dient die Kettenbremse?",
                    new List<QuestionOption>
                    {
                        new("q2o1", "Sie macht den Motor leiser"),
                        new("q2o2", "Sie stoppt die Kette in Gefahrensituationen"),
                        new("q2o3", "Sie ersetzt die Schutzkleidung")
                    },
                    "q2o2",
                    "Die Kettenbremse ist ein zentrales Sicherheitsmerkmal bei Rueckschlag oder Notstopp."),
                new(
                    "q3",
                    "Welche Schutzkleidung ist für den Einsatz besonders wichtig?",
                    new List<QuestionOption>
                    {
                        new("q3o1", "Schnittschutzhose, Helm mit Visier und Gehoerschutz"),
                        new("q3o2", "Normale Alltagskleidung"),
                        new("q3o3", "Nur Handschuhe")
                    },
                    "q3o1",
                    "Komplette PSA (persoenliche Schutzausruestung) minimiert Verletzungsrisiken."),
                new(
                    "q4",
                    "Welche Schutzkleidung ist für den Einsatz besonders wichtig?",
                    new List<QuestionOption>
                    {
                        new("q4o1", "Schnittschutzhose, Helm mit Visier und Gehoerschutz"),
                        new("q4o2", "Normale Alltagskleidung"),
                        new("q4o3", "Nur Handschuhe")
                    },
                    "q4o1",
                    "Komplette PSA (persoenliche Schutzausruestung) minimiert Verletzungsrisiken.")
            });
    }
}
