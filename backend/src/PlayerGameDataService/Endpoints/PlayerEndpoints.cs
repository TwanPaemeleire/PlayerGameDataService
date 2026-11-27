using PlayerGameDataService.Data;
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

        group.MapPost("/register", async (PlayerService playerService, RegisterPlayerRequest registerRequest) =>
        {
           Dictionary<string, string[]> errors = new();
            if(string.IsNullOrWhiteSpace(registerRequest.PlayerName))
            {
                errors.Add("playerName", ["The name parameter can't be empty."]); 
            } 
            else if(!Player.IsValidName(registerRequest.PlayerName))
            {
                errors.Add("playerName", ["Name must be 3 to 5 letters or numbers."]);
            }
            if(errors.Count != 0) return Results.ValidationProblem(errors);

            RegisterPlayerResponse? response = await playerService.RegisterAsync(registerRequest);
            if(response is null) return Results.Conflict("This name is already taken.");
            return Results.CreatedAtRoute("GetPlayerStats", new { playerName = response.PlayerName }, response);
        }).WithName("RegisterPlayer").RequireRateLimiting(RateLimitPolicies.Register);
    }
}