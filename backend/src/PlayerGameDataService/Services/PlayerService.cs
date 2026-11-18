using Microsoft.EntityFrameworkCore;
using PlayerGameDataService.Data;
using PlayerGameDataService.Models;

namespace PlayerGameDataService.Services;

public sealed class PlayerService(LeaderboardDbContext db)
{
    public async Task<ScoreResponse?> GetBestScoreAsync(string playerName)
    {
        return await db.Scores
            .Where(s => s.Player.Name == playerName)
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.SubmittedAt)
            .Select(ScoreResponse.FromScore)
            .FirstOrDefaultAsync();
    }

    public async Task<PlayerStatsResponse?> GetStatsAsync(string playerName)
    {
        var stats = await db.Players
            .Where(p => p.Name == playerName)
            .Select(p => new {
                PlayerName = p.Name,
                GamesPlayed = p.Scores.Count,
                BestScore = p.Scores.Max(playerScore => (int?)playerScore.Value),
                AverageScore = p.Scores.Average(playerScore => (double?)playerScore.Value)
                })
            .FirstOrDefaultAsync();
        
        if(stats == null) return null;

        int? rank = null;
        if(stats.GamesPlayed != 0)
        {
            int amountBetter = await db.Players.CountAsync(p => p.Scores.Max(s => (int?)s.Value) > stats.BestScore);
            rank = amountBetter + 1;
        }

        int? avgScore = stats.AverageScore is null ? null : (int)Math.Round(stats.AverageScore.Value, MidpointRounding.AwayFromZero);
        return new PlayerStatsResponse(stats.PlayerName, stats.GamesPlayed, stats.BestScore, avgScore, rank);
    }
}