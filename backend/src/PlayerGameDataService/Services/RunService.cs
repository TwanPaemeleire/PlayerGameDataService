using Microsoft.EntityFrameworkCore;
using PlayerGameDataService.Data;
namespace PlayerGameDataService.Services;

public sealed class RunService(LeaderboardDbContext db)
{
    private const int MaxPointsPerSecond = 300;
    private const int ScoreMargin = 1000;

    public async Task<Guid> StartAsync()
    {
        DateTime now = DateTime.UtcNow;
        await db.Runs.Where(r => r.StartedAt < now.AddDays(-1)).ExecuteDeleteAsync();

        Run run = new Run { Id = Guid.CreateVersion7(), StartedAt = now };
        db.Runs.Add(run);
        await db.SaveChangesAsync();
        return run.Id;
    }

    public async Task<bool> TryFinishAsync(Guid runId, int score)
    {
        DateTime? startedAt = await db.Runs.Where(r => r.Id == runId).Select(r => (DateTime?)r.StartedAt).FirstOrDefaultAsync();
        if (startedAt is null) return false;

        double secondsPlayed = (DateTime.UtcNow - startedAt.Value).TotalSeconds;
        if (score > secondsPlayed * MaxPointsPerSecond + ScoreMargin) return false;

        return await db.Runs.Where(r => r.Id == runId).ExecuteDeleteAsync() == 1;
    }
}
