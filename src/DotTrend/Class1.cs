namespace DotTrend;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

public class Trend<T> where T : class
{
    private IQueryable<T> _query;
    private DateTime _start;
    private DateTime _end;
    private string _dateColumn = "CreatedAt";
    private string _interval = "day";

    public Trend(IQueryable<T> query)
    {
        _query = query;
    }

    public static Trend<T> Query(IQueryable<T> query) => new Trend<T>(query);

    public Trend<T> Between(DateTime start, DateTime end)
    {
        _start = start;
        _end = end;
        return this;
    }

    public Trend<T> Interval(string interval)
    {
        _interval = interval.ToLower();
        return this;
    }

    public Trend<T> PerDay() => Interval("day");
    public Trend<T> PerMonth() => Interval("month");
    public Trend<T> PerYear() => Interval("year");

    public Trend<T> DateColumn(string column)
    {
        _dateColumn = column;
        return this;
    }

    private List<TrendResult> Aggregate(Expression<Func<T, decimal>> valueSelector, Func<IQueryable<T>, IQueryable<AggregationResult>> aggregationFunction)
    {
        // Filter the query for the specified date range
        var filteredQuery = _query.Where(entity =>
            EF.Property<DateTime>(entity, _dateColumn) >= _start &&
            EF.Property<DateTime>(entity, _dateColumn) <= _end);

        // Group by the specified time interval
        var groupedQuery = filteredQuery
    .GroupBy(entity => new
    {
        Period = _interval switch
        {
            "day" => EF.Property<DateTime>(entity, _dateColumn).Date,
            "month" => new DateTime(
                        EF.Property<DateTime>(entity, _dateColumn).Year,
                        EF.Property<DateTime>(entity, _dateColumn).Month,
                        1),
            "year" => new DateTime(
                        EF.Property<DateTime>(entity, _dateColumn).Year,
                        1,
                        1),
            _ => throw new NotSupportedException($"Interval '{_interval}' is not supported.")
        }
    });

        // Apply the aggregation function
        var aggregationQuery = aggregationFunction(groupedQuery.Select(g => new AggregationInput
        {
            Period = g.Key.Period,
            Value = g.Select(valueSelector).FirstOrDefault()
        }));

        // Execute the query and map to TrendResult
        return aggregationQuery.Select(ar => new TrendResult
        {
            Period = ar.Period,
            Aggregate = ar.Aggregate
        }).ToList();
    }

    public List<TrendResult> Count()
    {
        return Aggregate(null, groupedQuery => groupedQuery.Select(g => new AggregationResult
        {
            Period = g.Period,
            Aggregate = g.Count()
        }));
    }

    public List<TrendResult> Sum(Expression<Func<T, decimal>> valueSelector)
    {
        return Aggregate(valueSelector, groupedQuery => groupedQuery.Select(g => new AggregationResult
        {
            Period = g.Period,
            Aggregate = g.Sum(ai => ai.Value)
        }));
    }

    public List<TrendResult> Average(Expression<Func<T, decimal>> valueSelector)
    {
        return Aggregate(valueSelector, groupedQuery => groupedQuery.Select(g => new AggregationResult
        {
            Period = g.Period,
            Aggregate = g.Average(ai => ai.Value)
        }));
    }

    public List<TrendResult> Min(Expression<Func<T, decimal>> valueSelector)
    {
        return Aggregate(valueSelector, groupedQuery => groupedQuery.Select(g => new AggregationResult
        {
            Period = g.Period,
            Aggregate = g.Min(ai => ai.Value)
        }));
    }

    public List<TrendResult> Max(Expression<Func<T, decimal>> valueSelector)
    {
        return Aggregate(valueSelector, groupedQuery => groupedQuery.Select(g => new AggregationResult
        {
            Period = g.Period,
            Aggregate = g.Max(ai => ai.Value)
        }));
    }
}

public class TrendResult
{
    public DateTime Period { get; set; }
    public decimal Aggregate { get; set; }
}

public class AggregationInput
{
    public DateTime Period { get; set; }
    public decimal Value { get; set; }
}

public class AggregationResult
{
    public DateTime Period { get; set; }
    public decimal Aggregate { get; set; }
}