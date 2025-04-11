using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace DotTrend.Tests;

public class TrendTests
{
    private TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique name for each test
            .Options;

        var context = new TestDbContext(options);
        return context;
    }

    private void SeedSampleData(TestDbContext context)
    {
        var baseDate = new DateTime(2025, 1, 1);
        
        // Clear existing data
        context.Orders.RemoveRange(context.Orders);
        context.SaveChanges();

        // Add sample data across different days, with varying amounts
        var orders = new List<TestOrder>
        {
            // January orders
            new TestOrder { Id = 1, CreatedAt = baseDate.AddDays(0), Amount = 100, ProductCount = 1, Status = "Completed" },
            new TestOrder { Id = 2, CreatedAt = baseDate.AddDays(0), Amount = 200, ProductCount = 2, Status = "Completed" },
            new TestOrder { Id = 3, CreatedAt = baseDate.AddDays(1), Amount = 150, ProductCount = 3, Status = "Completed" },
            new TestOrder { Id = 4, CreatedAt = baseDate.AddDays(2), Amount = 120, ProductCount = 1, Status = "Pending" },
            new TestOrder { Id = 5, CreatedAt = baseDate.AddDays(3), Amount = 180, ProductCount = 2, Status = "Completed" },
            new TestOrder { Id = 6, CreatedAt = baseDate.AddDays(4), Amount = 90, ProductCount = 1, Status = "Completed" },
            
            // February orders
            new TestOrder { Id = 7, CreatedAt = baseDate.AddDays(32), Amount = 300, ProductCount = 3, Status = "Completed" },
            new TestOrder { Id = 8, CreatedAt = baseDate.AddDays(33), Amount = 250, ProductCount = 2, Status = "Completed" },
            new TestOrder { Id = 9, CreatedAt = baseDate.AddDays(35), Amount = 175, ProductCount = 1, Status = "Pending" },
            
            // Add orders with specific hours for hour/minute interval testing
            new TestOrder { Id = 10, CreatedAt = new DateTime(2025, 1, 15, 9, 15, 0), Amount = 110, ProductCount = 1, Status = "Completed" },
            new TestOrder { Id = 11, CreatedAt = new DateTime(2025, 1, 15, 9, 30, 0), Amount = 220, ProductCount = 2, Status = "Completed" },
            new TestOrder { Id = 12, CreatedAt = new DateTime(2025, 1, 15, 10, 15, 0), Amount = 330, ProductCount = 3, Status = "Completed" },
            new TestOrder { Id = 13, CreatedAt = new DateTime(2025, 1, 15, 11, 0, 0), Amount = 440, ProductCount = 4, Status = "Pending" }
        };

        context.Orders.AddRange(orders);
        context.SaveChanges();
    }

    [Fact]
    public void Count_PerDay_GroupsDataCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);

        // Act
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerDay()
            .Count();

        // Assert
        Assert.Equal(5, result.Count); // 5 days in range
        Assert.Equal(2, result[0].Aggregate); // 2 orders on day 1
        Assert.Equal(1, result[1].Aggregate); // 1 order on day 2
        Assert.Equal(1, result[2].Aggregate); // 1 order on day 3
        Assert.Equal(1, result[3].Aggregate); // 1 order on day 4
        Assert.Equal(1, result[4].Aggregate); // 1 order on day 5
    }

    [Fact]
    public void Sum_PerDay_CalculatesCorrectTotals()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);

        // Act
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerDay()
            .Sum(o => o.Amount);

        // Assert
        Assert.Equal(5, result.Count); // 5 days in range
        Assert.Equal(300, result[0].Aggregate); // Day 1 total
        Assert.Equal(150, result[1].Aggregate); // Day 2 total
        Assert.Equal(120, result[2].Aggregate); // Day 3 total
        Assert.Equal(180, result[3].Aggregate); // Day 4 total
        Assert.Equal(90, result[4].Aggregate); // Day 5 total
    }

    [Fact]
    public void Average_PerDay_CalculatesCorrectAverages()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);

        // Act
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerDay()
            .Average(o => o.Amount);

        // Assert
        Assert.Equal(5, result.Count); // 5 days in range
        Assert.Equal(150, result[0].Aggregate); // Day 1 average (300/2)
        Assert.Equal(150, result[1].Aggregate); // Day 2 average (150/1)
        Assert.Equal(120, result[2].Aggregate); // Day 3 average (120/1)
        Assert.Equal(180, result[3].Aggregate); // Day 4 average (180/1)
        Assert.Equal(90, result[4].Aggregate); // Day 5 average (90/1)
    }

    [Fact]
    public void Min_PerDay_FindsMinimumValues()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 2);

        // Act
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerDay()
            .Min(o => o.Amount);

        // Assert
        Assert.Equal(2, result.Count); // 2 days in range
        Assert.Equal(100, result[0].Aggregate); // Day 1 min
        Assert.Equal(150, result[1].Aggregate); // Day 2 min
    }

    [Fact]
    public void Max_PerDay_FindsMaximumValues()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 2);

        // Act
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerDay()
            .Max(o => o.Amount);

        // Assert
        Assert.Equal(2, result.Count); // 2 days in range
        Assert.Equal(200, result[0].Aggregate); // Day 1 max
        Assert.Equal(150, result[1].Aggregate); // Day 2 max
    }

    [Fact]
    public void Count_PerMonth_GroupsDataCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 2, 28);

        // Act
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerMonth()
            .Count();

        // Assert
        Assert.Equal(2, result.Count); // 2 months in range
        Assert.Equal(10, result[0].Aggregate); // January orders
        Assert.Equal(3, result[1].Aggregate); // February orders
    }

    [Fact]
    public void Count_PerHour_GroupsDataCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 15, 9, 0, 0);
        var endDate = new DateTime(2025, 1, 15, 11, 59, 59);

        // Act
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerHour()
            .Count();

        // Assert
        Assert.Equal(3, result.Count); // 3 hours in range (9, 10, 11)
        Assert.Equal(2, result[0].Aggregate); // 2 orders in hour 9
        Assert.Equal(1, result[1].Aggregate); // 1 order in hour 10
        Assert.Equal(1, result[2].Aggregate); // 1 order in hour 11
    }

    [Fact]
    public void FillsEmptyPeriodsWithZeros()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        // Add a gap in the data (no orders for Jan 10)
        var startDate = new DateTime(2025, 1, 8);
        var endDate = new DateTime(2025, 1, 12);

        // Act - there should be no actual data in this range but we should get filled zeros
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .PerDay()
            .Count();

        // Assert
        Assert.Equal(5, result.Count); // 5 days in range
        Assert.Equal(0, result[0].Aggregate); // No orders on Jan 8
        Assert.Equal(0, result[1].Aggregate); // No orders on Jan 9
        Assert.Equal(0, result[2].Aggregate); // No orders on Jan 10
        Assert.Equal(0, result[3].Aggregate); // No orders on Jan 11
        Assert.Equal(0, result[4].Aggregate); // No orders on Jan 12
    }

    [Fact]
    public void CustomDateColumn_Works()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);

        // Act - use explicit DateColumn method
        var result = Trend<TestOrder>
            .Query(context.Orders)
            .Between(startDate, endDate)
            .DateColumn("CreatedAt") // Explicitly set column name
            .PerDay()
            .Count();

        // Assert
        Assert.Equal(5, result.Count); // 5 days in range
        Assert.Equal(2, result[0].Aggregate); // 2 orders on day 1
    }

    [Fact]
    public void DifferentDatabaseAdapters_Work()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);

        // Act - test with different adapters
        var sqlServerResult = Trend<TestOrder>
            .Query(context.Orders, new SqlServerAdapter())
            .Between(startDate, endDate)
            .PerDay()
            .Count();
            
        var sqliteResult = Trend<TestOrder>
            .Query(context.Orders, new SqliteAdapter())
            .Between(startDate, endDate)
            .PerDay()
            .Count();
            
        var mySqlResult = Trend<TestOrder>
            .Query(context.Orders, new MySqlAdapter())
            .Between(startDate, endDate)
            .PerDay()
            .Count();
            
        var postgresResult = Trend<TestOrder>
            .Query(context.Orders, new PostgresAdapter())
            .Between(startDate, endDate)
            .PerDay()
            .Count();

        // Assert - all adapters should give same results with in-memory database
        Assert.Equal(5, sqlServerResult.Count);
        Assert.Equal(5, sqliteResult.Count);
        Assert.Equal(5, mySqlResult.Count);
        Assert.Equal(5, postgresResult.Count);
        
        Assert.Equal(2, sqlServerResult[0].Aggregate);
        Assert.Equal(2, sqliteResult[0].Aggregate);
        Assert.Equal(2, mySqlResult[0].Aggregate);
        Assert.Equal(2, postgresResult[0].Aggregate);
    }

    [Fact]
    public void StaticBetweenMethod_Works_WithGlobalContext()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        // Set the global DbContext for use with static methods
        context.UseTrend();
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);

        // Act - use the static Between method directly
        var result = Trend<TestOrder>
            .Between(startDate, endDate)
            .PerDay()
            .Count();

        // Assert
        Assert.Equal(5, result.Count); // 5 days in range
        Assert.Equal(2, result[0].Aggregate); // 2 orders on day 1
    }

    [Fact]
    public void ExtensionMethod_Works_WithIQueryable()
    {
        // Arrange
        using var context = CreateDbContext();
        SeedSampleData(context);
        
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);

        // Act - use the extension method on IQueryable
        var result = context.Orders
            .Trend()
            .Between(startDate, endDate)
            .PerDay()
            .Count();

        // Assert
        Assert.Equal(5, result.Count); // 5 days in range
        Assert.Equal(2, result[0].Aggregate); // 2 orders on day 1
    }

    [Fact]
    public void GetTrendAdapter_ReturnsCorrectAdapter()
    {
        // Arrange
        using var context = CreateDbContext();
        
        // Act - get adapter from context
        var adapter = context.GetTrendAdapter();
        
        // Assert - for in-memory database, we get the SQL Server adapter by default
        Assert.IsType<SqlServerAdapter>(adapter);
    }
}