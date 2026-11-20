namespace PlayerGameDataService.Data;

public sealed class Player
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<Score> Scores { get; set; } = [];
    public const int MaxNameLength = 20;
    public string? TokenHash {get; set;}
    public required string NormalizedName { get; set; }

    public static string NormalizeName(string name) => name.ToUpperInvariant();
}