using PlayerGameDataService.Models;
namespace PlayerGameDataService.Services;

public sealed class ScoreService
{
    private readonly Lock _scoresListLock = new();
    private readonly List<Score> _scores = 
    [
        new Score(Guid.CreateVersion7(), 200, "Me", DateTimeOffset.UtcNow),
        new Score(Guid.CreateVersion7(), 400, "Myself", DateTimeOffset.UtcNow.AddDays(-1))
    ];

    public IReadOnlyList<Score> GetAll()
    {
        lock(_scoresListLock)
        {
            return _scores.ToList();   
        }
    }

    public Score Add(SubmitScoreRequest request)
    {
        Score newScore = new Score(Guid.CreateVersion7(), request.Value, request.PlayerName, DateTimeOffset.UtcNow);
        lock(_scoresListLock)
        {
            _scores.Add(newScore);
        }
        return newScore;
    }

    public Score? FindById(Guid id)
    {
        lock(_scoresListLock)
        {
            return _scores.Find(score => score.Id == id);
        }
    }
}