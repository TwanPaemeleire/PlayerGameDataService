namespace PlayerGameDataService.Models;

public record PlayerStatsResponse(string PlayerName, int GamesPlayed, int? BestScore, int? AverageScore, int? Rank);