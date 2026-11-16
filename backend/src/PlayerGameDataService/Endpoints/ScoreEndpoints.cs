using PlayerGameDataService.Models;
using PlayerGameDataService.Services;
namespace PlayerGameDataService.Endpoints;

public static class ScoreEndpoints
{
    private const int TopRangeStart = 1;
    private const int TopRangeEnd = 100;
    private const int MaxNameCharLength = 20;

    public static void MapScoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/scores");

        group.MapGet("/", (ScoreService scoreService) =>
        {
            return scoreService.GetAll();
        }).WithName("GetScores");

        group.MapGet("/{id:guid}", (Guid id, ScoreService scoreService) =>
        {
            Score? foundScore = scoreService.FindById(id);
            return (foundScore == null) ? Results.NotFound() : Results.Ok(foundScore);
        }).WithName("GetScoreById");

        group.MapPost("/", (SubmitScoreRequest request, ScoreService scoreService) =>
        {
            Dictionary<string, string[]> errors = new();
            if (string.IsNullOrWhiteSpace(request.PlayerName))
            {
                errors.Add("playerName", ["A player name is required."]);
            }
            else if (request.PlayerName.Length > MaxNameCharLength)
            {
                errors.Add("playerName", [$"Name is too long, max length is {MaxNameCharLength}"]);
            }
            if (request.Value < 0)
            {
                errors.Add("value", ["The score value can't be below 0."]);
            }
            if (errors.Count != 0) return Results.ValidationProblem(errors);

            Score newScore = scoreService.Add(request);
            return Results.CreatedAtRoute("GetScoreById", new { id = newScore.Id }, newScore);
        }).WithName("SubmitScore");

        group.MapGet("/top", (ScoreService scoreService, int count = 10) =>
        {
            Dictionary<string, string[]> errors = new();
            if (count < TopRangeStart || count > TopRangeEnd)
            {
                errors.Add("count", [$"Invalid count requested. Allowed range is {TopRangeStart}-{TopRangeEnd}."]);
            }
            if (errors.Count != 0) return Results.ValidationProblem(errors);

            return Results.Ok(scoreService.GetTop(count));
        }).WithName("GetTopScores");
    }
}
