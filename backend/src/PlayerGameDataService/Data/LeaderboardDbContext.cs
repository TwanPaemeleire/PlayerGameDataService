using Microsoft.EntityFrameworkCore;
namespace PlayerGameDataService.Data;

public sealed class LeaderboardDbContext(DbContextOptions<LeaderboardDbContext> options)
: DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Score> Scores => Set<Score>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>().HasIndex(p => p.Name).IsUnique();
        modelBuilder.Entity<Player>().Property(p => p.Name).HasMaxLength(Player.MaxNameLength);
    }
}