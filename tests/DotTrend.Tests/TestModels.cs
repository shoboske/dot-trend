using Microsoft.EntityFrameworkCore;

namespace DotTrend.Tests;

public class TestOrder
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Amount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class TestDbContext : DbContext
{
    public DbSet<TestOrder> Orders { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestOrder>()
            .HasKey(o => o.Id);

        base.OnModelCreating(modelBuilder);
    }
}