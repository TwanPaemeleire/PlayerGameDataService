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
            .Select(ScoreResponse.FromScore)
            .ToListAsync();
    }

    public async Task<ScoreResponse> AddAsync(Guid playerId, int value)
    {
        Score newScore = new Score
        {
            Id = Guid.CreateVersion7(),
            PlayerId = playerId,
            SubmittedAt = DateTime.UtcNow,
            Value = value
        };
        db.Scores.Add(newScore);
        await db.SaveChangesAsync();

        return (await FindByIdAsync(newScore.Id))!;
    }

    public async Task<ScoreResponse?> FindByIdAsync(Guid id)
    {
        return await db.Scores
            .Where(s => s.Id == id)
            .Select(ScoreResponse.FromScore)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ScoreResponse>> GetTopAsync(int count)
    {
        return await db.Players
            .Where(p => p.Scores.Any())
            .Select(p => p.Scores
                .OrderByDescending(s => s.Value)
                .ThenBy(s => s.SubmittedAt)
                .First())
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.SubmittedAt)
            .Take(count)
            .Select(ScoreResponse.FromScore)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ScoreResponse>> GetTopWeeklyAsync(int count)
    {
        var since = DateTime.UtcNow.AddDays(-7);
        return await db.Players
            .Where(p => p.Scores.Any(s => s.SubmittedAt >= since))
            .Select(p => p.Scores
                .Where(s => s.SubmittedAt >= since)
                .OrderByDescending(s => s.Value)
                .ThenBy(s => s.SubmittedAt)
                .First())
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.SubmittedAt)
            .Take(count)
            .Select(ScoreResponse.FromScore)
            .ToListAsync();
    }
}