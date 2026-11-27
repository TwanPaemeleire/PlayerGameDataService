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
    public static bool IsValidName(string name) => name.Length is >= 3 and <= 5 && name.All(char.IsLetterOrDigit);
}