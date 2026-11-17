namespace PlayerGameDataService.Models;
public record ScoreResponse(Guid Id, int Value, string PlayerName, DateTimeOffset SubmittedAt);
