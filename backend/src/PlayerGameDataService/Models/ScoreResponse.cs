using System.Linq.Expressions;
using PlayerGameDataService.Data;

namespace PlayerGameDataService.Models;
public record ScoreResponse(Guid Id, int Value, string PlayerName, DateTimeOffset SubmittedAt)
{
    public static readonly Expression<Func<Score, ScoreResponse>> FromScore = s =>
        new ScoreResponse(s.Id, s.Value, s.Player.Name, new DateTimeOffset(s.SubmittedAt, TimeSpan.Zero));
}
