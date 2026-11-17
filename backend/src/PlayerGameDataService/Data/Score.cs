namespace PlayerGameDataService.Data;

public sealed class Score
{
    public Guid Id { get; set; }
    public int Value { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
}