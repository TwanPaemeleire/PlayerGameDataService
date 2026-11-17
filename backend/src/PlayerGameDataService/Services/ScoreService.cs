using Microsoft.EntityFrameworkCore;
using PlayerGameDataService.Data;
using PlayerGameDataService.Models;
namespace PlayerGameDataService.Services;

public sealed class ScoreService(LeaderboardDbContext db)
{
    public async Task<IReadOnlyList<ScoreResponse>> GetAllAsync()
    {
        return await db.Scores
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new ScoreResponse(
                s.Id,
                s.Value,
                s.Player.Name,
                new DateTimeOffset(s.SubmittedAt, TimeSpan.Zero)))
            .ToListAsync();
    }

    public async Task<ScoreResponse> AddAsync(SubmitScoreRequest request)
    {
        Player? playerResult = await db.Players.FirstOrDefaultAsync(p => p.Name == request.PlayerName);
        if (playerResult is null)
        {
            playerResult = new Player { Id = Guid.CreateVersion7(), Name = request.PlayerName };
            db.Players.Add(playerResult);
        }

        Score newScore = new Score
        {
            Id = Guid.CreateVersion7(),
            Player = playerResult,
            PlayerId = playerResult.Id,
            SubmittedAt = DateTime.UtcNow,
            Value = request.Value
        };
        db.Scores.Add(newScore);
        await db.SaveChangesAsync();

        return new ScoreResponse(newScore.Id,
                                 newScore.Value,
                                 newScore.Player.Name,
                                 new DateTimeOffset(newScore.SubmittedAt, TimeSpan.Zero));
    }

    public async Task<ScoreResponse?> FindByIdAsync(Guid id)
    {
        return await db.Scores
            .Where(s => s.Id == id)
            .Select(s => new ScoreResponse(
                s.Id,
                s.Value,
                s.Player.Name,
                new DateTimeOffset(s.SubmittedAt, TimeSpan.Zero)))
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ScoreResponse>> GetTopAsync(int count)
    {
        return await db.Scores
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.SubmittedAt)
            .Select(s => new ScoreResponse(
                s.Id,
                s.Value,
                s.Player.Name,
                new DateTimeOffset(s.SubmittedAt, TimeSpan.Zero)))
            .Take(count)
            .ToListAsync();
    }
}