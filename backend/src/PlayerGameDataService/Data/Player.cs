namespace PlayerGameDataService.Data;

public sealed class Player
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<Score> Scores { get; set; } = [];
}