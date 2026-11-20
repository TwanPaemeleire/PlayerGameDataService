using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PlayerGameDataService.Data;
using PlayerGameDataService.Models;

namespace PlayerGameDataService.Services;

public sealed class PlayerService(LeaderboardDbContext db)
{
    public async Task<ScoreResponse?> GetBestScoreAsync(string playerName)
    {
        string normalizedName = Player.NormalizeName(playerName);
        return await db.Scores
            .Where(s => s.Player.NormalizedName == normalizedName)
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.SubmittedAt)
            .Select(ScoreResponse.FromScore)
            .FirstOrDefaultAsync();
    }

    public async Task<PlayerStatsResponse?> GetStatsAsync(string playerName)
    {
        string normalizedName = Player.NormalizeName(playerName);
        var stats = await db.Players
            .Where(p => p.NormalizedName == normalizedName)
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

    public async Task<RegisterPlayerResponse?> RegisterAsync(RegisterPlayerRequest request)
    {
        string normalizedName = Player.NormalizeName(request.PlayerName);
        bool nameTaken = await db.Players.AnyAsync(p => p.NormalizedName == normalizedName);
        if (nameTaken) return null;

        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        Player player = new Player
        {
            Id = Guid.CreateVersion7(),
            Name = request.PlayerName,
            NormalizedName = normalizedName,
            TokenHash = HashToken(token)
        };
        db.Players.Add(player);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return new RegisterPlayerResponse(player.Name, token);
    }

    public async Task<Guid?> FindIdByTokenAsync(string token)
    {
        string tokenHash = HashToken(token);
        return await db.Players
            .Where(p => p.TokenHash == tokenHash)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync();
    }

    public static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}