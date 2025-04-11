# DotTrend

[![NuGet](https://img.shields.io/nuget/v/DotTrend.svg)](https://www.nuget.org/packages/DotTrend/)
[![License](https://img.shields.io/github/license/shoboske/dot-trend)](https://github.com/shoboske/dot-trend/blob/main/LICENSE)
[![.NET CI](https://github.com/shoboske/dot-trend/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/yourusername/dot-trend/actions/workflows/dotnet-ci.yml)

A .NET library for generating time-series trend data from Entity Framework Core queries with support for different database providers. DotTrend makes it easy to create trend reports by date intervals (minute, hour, day, week, month, year) with zero-filling for missing periods.

> **Inspiration**: DotTrend is heavily inspired by the [Laravel Trend](https://github.com/Flowframe/laravel-trend) package, bringing similar time-series trend generation capabilities to the .NET ecosystem.

## Features

- 📊 Generate time-series trend data from your database
- 📅 Group by different time intervals (minute, hour, day, week, month, year)
- 📈 Aggregate data using various functions (count, sum, average, min, max)
- 🗄️ Support for multiple database providers:
  - SQL Server
  - MySQL
  - PostgreSQL
  - SQLite
- 🧩 Fluent API with multiple usage patterns for flexibility
- 0️⃣ Automatic zero-filling for missing periods

## Installation

Install the package via NuGet:

```sh
dotnet add package DotTrend
```

## Quick Start

```csharp
using DotTrend;
using System;
using System.Linq;

// Get daily order counts for January 2025
var dailyOrderCounts = Trend<Order>
    .Query(dbContext.Orders)
    .Between(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31))
    .PerDay()
    .Count();

// Generate monthly sales report for 2025
var monthlySales = Trend<Order>
    .Query(dbContext.Orders)
    .Between(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31))
    .PerMonth()
    .Sum(o => o.Amount);

// Display the results
foreach (var point in monthlySales)
{
    Console.WriteLine($"{point.Date:yyyy-MM}: ${point.Aggregate}");
}
```

## Usage Patterns

DotTrend supports multiple usage patterns to fit your coding style:

### 1. Traditional approach with Query method

```csharp
var result = Trend<Order>
    .Query(dbContext.Orders)
    .Between(startDate, endDate)
    .PerDay()
    .Count();
```

### 2. Extension method on IQueryable

```csharp
var result = dbContext.Orders
    .Trend()
    .Between(startDate, endDate)
    .PerDay() 
    .Count();
```

### 3. Static Between method (requires context registration)

First, register your DbContext:

```csharp
// In your application startup
dbContext.UseTrend();
```

Then use the static approach:

```csharp
var result = Trend<Order>
    .Between(startDate, endDate)
    .PerDay()
    .Count();
```

## Time Interval Options

```csharp
// Per minute (e.g., for real-time dashboards)
var minuteData = Trend<Event>.Query(dbContext.Events)
    .Between(startDate, endDate)
    .PerMinute()
    .Count();

// Per hour
var hourlyData = Trend<Event>.Query(dbContext.Events)
    .Between(startDate, endDate)
    .PerHour()
    .Count();

// Per day (default)
var dailyData = Trend<Event>.Query(dbContext.Events)
    .Between(startDate, endDate)
    .PerDay()
    .Count();

// Per week
var weeklyData = Trend<Event>.Query(dbContext.Events)
    .Between(startDate, endDate)
    .PerWeek()
    .Count();

// Per month
var monthlyData = Trend<Event>.Query(dbContext.Events)
    .Between(startDate, endDate)
    .PerMonth()
    .Count();

// Per year
var yearlyData = Trend<Event>.Query(dbContext.Events)
    .Between(startDate, endDate)
    .PerYear()
    .Count();
```

## Aggregation Functions

```csharp
// Count (default)
var orderCount = Trend<Order>.Query(dbContext.Orders)
    .Between(startDate, endDate)
    .PerDay()
    .Count();

// Sum
var totalSales = Trend<Order>.Query(dbContext.Orders)
    .Between(startDate, endDate)
    .PerDay()
    .Sum(o => o.Amount);

// Average
var averageOrderValue = Trend<Order>.Query(dbContext.Orders)
    .Between(startDate, endDate)
    .PerDay()
    .Average(o => o.Amount);

// Minimum
var minOrderValue = Trend<Order>.Query(dbContext.Orders)
    .Between(startDate, endDate)
    .PerDay()
    .Min(o => o.Amount);

// Maximum
var maxOrderValue = Trend<Order>.Query(dbContext.Orders)
    .Between(startDate, endDate)
    .PerDay()
    .Max(o => o.Amount);
```

## Database-Specific Adapters

```csharp
// SQL Server (default)
var sqlServerTrend = Trend<Order>.Query(
    dbContext.Orders,
    new SqlServerAdapter()
);

// MySQL
var mysqlTrend = Trend<Order>.Query(
    dbContext.Orders,
    new MySqlAdapter()
);

// PostgreSQL
var postgresTrend = Trend<Order>.Query(
    dbContext.Orders,
    new PostgresAdapter()
);

// SQLite
var sqliteTrend = Trend<Order>.Query(
    dbContext.Orders,
    new SqliteAdapter()
);

// Auto-detect database type from context
var autoAdapter = dbContext.GetTrendAdapter();
var autoTrend = Trend<Order>.Query(dbContext.Orders, autoAdapter);
```

## Custom Date Column

```csharp
// Use a different date column instead of the default "CreatedAt"
var ordersByProcessedDate = Trend<Order>.Query(dbContext.Orders)
    .Between(startDate, endDate)
    .DateColumn("ProcessedAt")
    .PerDay()
    .Count();
```

## Result Format

The result of any trend calculation is a list of `TrendValue` objects, which have these properties:

- `Date`: The DateTime representing the period
- `Aggregate`: The decimal value of the aggregation for that period

```csharp
public class TrendValue
{
    public DateTime Date { get; set; }
    public decimal Aggregate { get; set; }
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.