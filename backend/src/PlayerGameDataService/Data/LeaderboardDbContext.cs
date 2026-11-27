using Microsoft.EntityFrameworkCore;
namespace PlayerGameDataService.Data;

public sealed class LeaderboardDbContext(DbContextOptions<LeaderboardDbContext> options)
: DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Run> Runs => Set<Run>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>().HasIndex(p => p.Name).IsUnique();
        modelBuilder.Entity<Player>().Property(p => p.Name).HasMaxLength(Player.MaxNameLength);
        modelBuilder.Entity<Player>().HasIndex(p => p.TokenHash).IsUnique();
        modelBuilder.Entity<Player>().HasIndex(p => p.NormalizedName).IsUnique();
        modelBuilder.Entity<Player>().Property(p => p.NormalizedName).HasMaxLength(Player.MaxNameLength);
    }
}