using PlayerGameDataService.Models;
namespace PlayerGameDataService.Services;

public sealed class ScoreService
{
    private readonly Lock _scoresListLock = new();
    private readonly List<ScoreResponse> _scores =
    [
        new ScoreResponse(Guid.CreateVersion7(), 200, "Me", DateTimeOffset.UtcNow),
        new ScoreResponse(Guid.CreateVersion7(), 400, "Myself", DateTimeOffset.UtcNow.AddDays(-1))
    ];

    public IReadOnlyList<ScoreResponse> GetAll()
    {
        lock (_scoresListLock)
        {
            return _scores.ToList();
        }
    }

    public ScoreResponse Add(SubmitScoreRequest request)
    {
        ScoreResponse newScore = new ScoreResponse(Guid.CreateVersion7(), request.Value, request.PlayerName, DateTimeOffset.UtcNow);
        lock (_scoresListLock)
        {
            _scores.Add(newScore);
        }
        return newScore;
    }

    public ScoreResponse? FindById(Guid id)
    {
        lock (_scoresListLock)
        {
            return _scores.Find(score => score.Id == id);
        }
    }

    public IReadOnlyList<ScoreResponse> GetTop(int count)
    {
        lock (_scoresListLock)
        {
            return _scores.OrderByDescending(score => score.Value).ThenBy(score => score.SubmittedAt).Take(count).ToList();
        }
    }
}