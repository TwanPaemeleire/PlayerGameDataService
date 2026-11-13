namespace PlayerGameDataService.Models;
public record Score(Guid Id, int Value, string PlayerName, DateTimeOffset SubmittedAt);
