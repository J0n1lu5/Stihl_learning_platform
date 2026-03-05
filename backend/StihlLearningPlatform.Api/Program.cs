using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevClientPolicy = "AngularDevClient";

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevClientPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

var learningModule = LearningModuleFactory.CreateIntroModule();
var progressStore = new ConcurrentDictionary<string, UserProgress>();

app.UseHttpsRedirection();
app.UseCors(AngularDevClientPolicy);

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        service = "stihl-learning-platform-api",
        timestampUtc = DateTime.UtcNow
    });
});

app.MapGet("/api/modules/intro", () =>
{
    var moduleResponse = new LearningModuleDto(
        learningModule.Id,
        learningModule.Title,
        learningModule.Description,
        learningModule.Questions
            .Select(q => new QuizQuestionDto(
                q.Id,
                q.Prompt,
                q.Options.Select(o => new QuestionOptionDto(o.Id, o.Text)).ToList()))
            .ToList());

    return Results.Ok(moduleResponse);
});

app.MapPost("/api/modules/intro/submit", (SubmitQuizRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.UserId))
    {
        return Results.BadRequest(new { message = "userId is required." });
    }

    var questionResults = learningModule.Questions
        .Select(question =>
        {
            request.Answers.TryGetValue(question.Id, out var selectedOptionId);
            var isCorrect = string.Equals(selectedOptionId, question.CorrectOptionId, StringComparison.Ordinal);

            return new QuizAnswerResult(
                question.Id,
                selectedOptionId,
                question.CorrectOptionId,
                isCorrect,
                question.Explanation);
        })
        .ToList();

    var totalQuestions = learningModule.Questions.Count;
    var correctAnswers = questionResults.Count(q => q.IsCorrect);
    var scorePercent = Math.Round((double)correctAnswers / totalQuestions * 100, 1);
    var passed = scorePercent >= 70;

    var progress = progressStore.AddOrUpdate(
        request.UserId,
        key => UserProgress.CreateNew(key, scorePercent, passed),
        (_, existing) => existing.Update(scorePercent, passed));

    var response = new QuizSubmissionResponse(
        learningModule.Id,
        totalQuestions,
        correctAnswers,
        scorePercent,
        passed,
        questionResults,
        progress);

    return Results.Ok(response);
});

app.MapGet("/api/progress/{userId}", (string userId) =>
{
    if (string.IsNullOrWhiteSpace(userId))
    {
        return Results.BadRequest(new { message = "userId is required." });
    }

    if (progressStore.TryGetValue(userId, out var progress))
    {
        return Results.Ok(progress);
    }

    return Results.Ok(UserProgress.CreateEmpty(userId));
});

app.Run();
