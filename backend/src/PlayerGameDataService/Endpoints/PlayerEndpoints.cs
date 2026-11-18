using PlayerGameDataService.Models;
using PlayerGameDataService.Services;
namespace PlayerGameDataService.Endpoints;

public static class PlayerEndpoints
{
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/players");

        group.MapGet("/{playerName}/bestScore", async (PlayerService playerService, string playerName) =>
        {
            Dictionary<string, string[]> errors = new();
            if(string.IsNullOrWhiteSpace(playerName))
            {
                errors.Add("playerName", ["The name parameter can't be empty."]);
                return Results.ValidationProblem(errors);
            }

            ScoreResponse? response = await playerService.GetBestScoreAsync(playerName);
            if(response is null) return Results.NotFound("No scores found for this player.");
            return Results.Ok(response);
        }).WithName("GetPlayerBestScore");

        group.MapGet("/{playerName}/stats", async (PlayerService playerService, string playerName) =>
        {
            Dictionary<string, string[]> errors = new();
            if(string.IsNullOrWhiteSpace(playerName))
            {
                errors.Add("playerName", ["The name parameter can't be empty."]);
                return Results.ValidationProblem(errors);
            }

            PlayerStatsResponse? response = await playerService.GetStatsAsync(playerName);
            if(response is null) return Results.NotFound("The player doesn't exist.");
            return Results.Ok(response);
        }).WithName("GetPlayerStats");
    }
}