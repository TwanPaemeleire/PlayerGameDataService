using PlayerGameDataService.Models;
using PlayerGameDataService.Services;
namespace PlayerGameDataService.Endpoints;

public static class ScoreEndpoints
{
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
            Score newScore = scoreService.Add(request);
            return Results.CreatedAtRoute("GetScoreById", new { id = newScore.Id }, newScore);
        }).WithName("SubmitScore");
    }
}
