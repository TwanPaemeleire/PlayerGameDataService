using Microsoft.EntityFrameworkCore;
namespace PlayerGameDataService.Data;

public sealed class LeaderboardDbContext(DbContextOptions<LeaderboardDbContext> options)
: DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Score> Scores => Set<Score>();
}