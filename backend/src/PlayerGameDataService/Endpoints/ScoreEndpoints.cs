using PlayerGameDataService.Data;
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

        group.MapPost("/", async (SubmitScoreRequest request, ScoreService scoreService) =>
        {
            Dictionary<string, string[]> errors = new();
            if (string.IsNullOrWhiteSpace(request.PlayerName))
            {
                errors.Add("playerName", ["A player name is required."]);
            }
            else if (request.PlayerName.Length > Player.MaxNameLength)
            {
                errors.Add("playerName", [$"Name is too long, max length is {Player.MaxNameLength}"]);
            }
            if (request.Value < 0)
            {
                errors.Add("value", ["The score value can't be below 0."]);
            }
            if (errors.Count != 0) return Results.ValidationProblem(errors);

            ScoreResponse newScore = await scoreService.AddAsync(request);
            return Results.CreatedAtRoute("GetScoreById", new { id = newScore.Id }, newScore);
        }).WithName("SubmitScore");

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
