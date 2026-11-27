using PlayerGameDataService.Models;
using PlayerGameDataService.Services;
namespace PlayerGameDataService.Endpoints;

public static class ScoreEndpoints
{
    private const int TopRangeStart = 1;
    private const int TopRangeEnd = 100;

    public static void MapScoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/scores");

        group.MapGet("/", async (ScoreService scoreService) =>
        {
            return await scoreService.GetAllAsync();
        }).WithName("GetScores");

        group.MapGet("/{id:guid}", async (Guid id, ScoreService scoreService) =>
        {
            ScoreResponse? foundScore = await scoreService.FindByIdAsync(id);
            return (foundScore == null) ? Results.NotFound() : Results.Ok(foundScore);
        }).WithName("GetScoreById");

        group.MapPost("/", async (SubmitScoreRequest request, HttpRequest httpRequest, ScoreService scoreService, PlayerService playerService, RunService runService) =>
        {
            const string bearerPrefix = "Bearer ";
            string? authorization = httpRequest.Headers.Authorization;
            if (authorization is null || !authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            string token = authorization[bearerPrefix.Length..].Trim();
            Guid? playerId = string.IsNullOrEmpty(token) ? null : await playerService.FindIdByTokenAsync(token);
            if (playerId is null) return Results.Unauthorized();

            Dictionary<string, string[]> errors = new();
            if (request.Value < 0)
            {
                errors.Add("value", ["The score value can't be below 0."]);
            }
            else if (!await runService.TryFinishAsync(request.RunId, request.Value))
            {
                errors.Add("value", ["This score couldn't be verified."]);
            }
            if (errors.Count != 0) return Results.ValidationProblem(errors);

            ScoreResponse newScore = await scoreService.AddAsync(playerId.Value, request.Value);
            return Results.CreatedAtRoute("GetScoreById", new { id = newScore.Id }, newScore);
        }).WithName("SubmitScore").RequireRateLimiting(RateLimitPolicies.PerClient);

        group.MapGet("/top", async (ScoreService scoreService, int count = 10) =>
        {
            Dictionary<string, string[]> errors = new();
            if (count < TopRangeStart || count > TopRangeEnd)
            {
                errors.Add("count", [$"Invalid count requested. Allowed range is {TopRangeStart}-{TopRangeEnd}."]);
            }
            if (errors.Count != 0) return Results.ValidationProblem(errors);

            return Results.Ok(await scoreService.GetTopAsync(count));
        }).WithName("GetTopScores");

        group.MapGet("/top/weekly", async (ScoreService scoreService, int count = 10) =>
        {
            Dictionary<string, string[]> errors = new();
            if (count < TopRangeStart || count > TopRangeEnd)
            {
                errors.Add("count", [$"Invalid count requested. Allowed range is {TopRangeStart}-{TopRangeEnd}."]);
            }
            if (errors.Count != 0) return Results.ValidationProblem(errors);

            return Results.Ok(await scoreService.GetTopWeeklyAsync(count));
        }).WithName("GetWeeklyTopScores");
    }
}
