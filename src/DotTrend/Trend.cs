using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// Core class for generating time-series trend data from Entity Framework Core queries.
    /// Supports different time intervals and aggregation functions with automatic zero-filling.
    /// </summary>
    /// <typeparam name="TModel">The Entity Framework entity type to analyze</typeparam>
    public class Trend<TModel> where TModel : class
    {
        private readonly IQueryable<TModel> _query;
        private DateTime _start;
        private DateTime _end;
        private string _dateColumn = "CreatedAt";
        private string _interval = "day";
        private readonly IDatabaseAdapter _databaseAdapter;

        /// <summary>
        /// Creates a new Trend instance with the specified query and database adapter
        /// </summary>
        /// <param name="query">The IQueryable source of data</param>
        /// <param name="databaseAdapter">The database-specific adapter to use for date formatting</param>
        public Trend(IQueryable<TModel> query, IDatabaseAdapter databaseAdapter)
        {
            _query = query;
            _databaseAdapter = databaseAdapter;
        }

        /// <summary>
        /// Creates a new trend instance from an IQueryable source
        /// </summary>
        /// <param name="query">The IQueryable source to analyze</param>
        /// <param name="databaseAdapter">Optional database adapter (defaults to SQL Server)</param>
        /// <returns>A new Trend instance configured with the specified query</returns>
        public static Trend<TModel> Query(IQueryable<TModel> query, IDatabaseAdapter? databaseAdapter = null)
        {
            // Default to SqlServer if not specified
            databaseAdapter ??= new SqlServerAdapter();
            return new Trend<TModel>(query, databaseAdapter);
        }

        /// <summary>
        /// Creates a new trend instance from a DbSet
        /// </summary>
        /// <param name="dbSet">The DbSet to analyze</param>
        /// <param name="databaseAdapter">Optional database adapter (defaults to SQL Server)</param>
        /// <returns>A new Trend instance configured with the specified DbSet</returns>
        public static Trend<TModel> Model(DbSet<TModel> dbSet, IDatabaseAdapter? databaseAdapter = null)
        {
            return Query(dbSet, databaseAdapter);
        }
        
        /// <summary>
        /// Creates a new trend instance using the default DbContext.
        /// Note: This requires a DbContext to be available through dependency injection or a static property.
        /// </summary>
        /// <param name="databaseAdapter">Optional database adapter (defaults to SQL Server)</param>
        /// <returns>A new Trend instance</returns>
        /// <exception cref="InvalidOperationException">Thrown when no DbContext with DbSet<TModel> can be found</exception>
        public static Trend<TModel> Between(DateTime start, DateTime end, IDatabaseAdapter? databaseAdapter = null)
        {
            // Get the current DbContext from somewhere
            var dbSet = TrendDbContextProvider.GetDbSet<TModel>();
            if (dbSet == null)
            {
                throw new InvalidOperationException(
                    $"No DbSet of type {typeof(TModel).Name} could be found. " +
                    "Please use Trend<T>.Query() or set up a TrendDbContextProvider.");
            }
            
            return Query(dbSet, databaseAdapter).Between(start, end);
        }

        /// <summary>
        /// Sets the date range for the trend
        /// </summary>
        public Trend<TModel> Between(DateTime start, DateTime end)
        {
            _start = start;
            _end = end;
            return this;
        }

        /// <summary>
        /// Sets the grouping interval
        /// </summary>
        public Trend<TModel> Interval(string interval)
        {
            _interval = interval.ToLower();
            return this;
        }

        /// <summary>
        /// Sets the grouping interval to a specific time unit
        /// </summary>
        /// <returns></returns>
        public Trend<TModel> PerMinute() => Interval("minute");

        /// <summary>
        public Trend<TModel> PerHour() => Interval("hour");
        public Trend<TModel> PerDay() => Interval("day");
        public Trend<TModel> PerWeek() => Interval("week");
        public Trend<TModel> PerMonth() => Interval("month");
        public Trend<TModel> PerYear() => Interval("year");

        /// <summary>
        /// Sets the column to use for date grouping
        /// </summary>
        public Trend<TModel> DateColumn(string column)
        {
            _dateColumn = column;
            return this;
        }

        /// <summary>
        /// Performs a count aggregation
        /// </summary>
        public List<TrendValue> Count()
        {
            return Aggregate<object>(null, AggregateFunction.Count);
        }

        /// <summary>
        /// Performs a sum aggregation on the specified column
        /// </summary>
        public List<TrendValue> Sum<TValue>(Expression<Func<TModel, TValue>> valueSelector)
        {
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            return Aggregate(valueSelector, AggregateFunction.Sum);
        }

        /// <summary>
        /// Performs an average aggregation on the specified column
        /// </summary>
        public List<TrendValue> Average<TValue>(Expression<Func<TModel, TValue>> valueSelector)
        {
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            return Aggregate(valueSelector, AggregateFunction.Average);
        }

        /// <summary>
        /// Gets the minimum value for the specified column
        /// </summary>
        public List<TrendValue> Min<TValue>(Expression<Func<TModel, TValue>> valueSelector)
        {
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            return Aggregate(valueSelector, AggregateFunction.Min);
        }

        /// <summary>
        /// Gets the maximum value for the specified column
        /// </summary>
        public List<TrendValue> Max<TValue>(Expression<Func<TModel, TValue>> valueSelector)
        {
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            return Aggregate(valueSelector, AggregateFunction.Max);
        }

        /// <summary>
        /// Core method that performs the aggregation and returns trend values
        /// </summary>
        private List<TrendValue> Aggregate<TValue>(Expression<Func<TModel, TValue>>? valueSelector, AggregateFunction function)
        {
            // Create a parameter expression for the entity type
            var parameter = Expression.Parameter(typeof(TModel), "e");

            // Create a property access expression for the date column
            var dateProperty = Expression.Property(parameter, _dateColumn);
            
            // Create a lambda expression for accessing the date property
            var datePropertyLambda = Expression.Lambda<Func<TModel, DateTime>>(dateProperty, parameter);

            // Filter the query for the specified date range
            var filteredQuery = _query.Where(entity =>
                EF.Property<DateTime>(entity, _dateColumn) >= _start &&
                EF.Property<DateTime>(entity, _dateColumn) <= _end);

            // Get the formatted date expression based on the interval
            var formattedDateExpression = _databaseAdapter.FormatDate(datePropertyLambda, _interval);

            // Create a selector expression for the formatted date
            var periodSelector = CreateTrendPeriodSelector(parameter);

            // Perform the grouping by formatted date
            var groupedQuery = filteredQuery.GroupBy(periodSelector, entity => entity);

            // Apply the aggregation function
            var results = ApplyAggregation(groupedQuery, valueSelector, function);

            // Fill in missing periods
            return FillMissingPeriods(results);
        }

        /// <summary>
        /// Creates a selector expression for the TrendPeriod
        /// </summary>
        private Expression<Func<TModel, TrendPeriod>> CreateTrendPeriodSelector(ParameterExpression parameter)
        {
            // Create a parameter expression for the entity type if not provided
            if (parameter == null)
                parameter = Expression.Parameter(typeof(TModel), "e");

            // Create a property access expression for the date column
            var dateProperty = Expression.Property(parameter, _dateColumn);

            // Create expressions for the different period components
            var yearExpr = Expression.Property(dateProperty, "Year");
            var monthExpr = Expression.Property(dateProperty, "Month");
            var dayExpr = Expression.Property(dateProperty, "Day");
            var hourExpr = Expression.Property(dateProperty, "Hour");
            var minuteExpr = Expression.Property(dateProperty, "Minute");

            // Create a new expression to construct a TrendPeriod
            var constructor = typeof(TrendPeriod).GetConstructor(new[] { 
                typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(string) 
            }) ?? throw new InvalidOperationException("Could not find TrendPeriod constructor");
            var intervalExpr = Expression.Constant(_interval);

            var newExpr = Expression.New(constructor, 
                yearExpr, monthExpr, dayExpr, hourExpr, minuteExpr, intervalExpr);

            // Create and return the lambda expression
            return Expression.Lambda<Func<TModel, TrendPeriod>>(newExpr, parameter);
        }

        /// <summary>
        /// Applies the specified aggregation function to the grouped query
        /// </summary>
        private List<TrendValue> ApplyAggregation<TValue>(
            IQueryable<IGrouping<TrendPeriod, TModel>> groupedQuery, 
            Expression<Func<TModel, TValue>>? valueSelector,
            AggregateFunction function)
        {
            if (function == AggregateFunction.Count)
            {
                return groupedQuery
                    .Select(g => new TrendValue
                    {
                        Date = g.Key.ToDateTime(),
                        Aggregate = g.Count()
                    })
                    .OrderBy(r => r.Date)
                    .ToList();
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector), 
                    "Value selector cannot be null for Sum, Average, Min, or Max operations");
            }

            // Extract the property name from the value selector
            string? propertyName = null;
            if (valueSelector.Body is MemberExpression memberExpr)
            {
                propertyName = memberExpr.Member.Name;
            }
            
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new InvalidOperationException(
                    "Could not determine property name from value selector");
            }

            return function switch
            {
                AggregateFunction.Sum => groupedQuery
                    .Select(g => new TrendValue
                    {
                        Date = g.Key.ToDateTime(),
                        Aggregate = g.Sum(e => Convert.ToDecimal(EF.Property<object>(e, propertyName)))
                    })
                    .OrderBy(r => r.Date)
                    .ToList(),

                AggregateFunction.Average => groupedQuery
                    .Select(g => new TrendValue
                    {
                        Date = g.Key.ToDateTime(),
                        Aggregate = g.Average(e => Convert.ToDecimal(EF.Property<object>(e, propertyName)))
                    })
                    .OrderBy(r => r.Date)
                    .ToList(),

                AggregateFunction.Min => groupedQuery
                    .Select(g => new TrendValue
                    {
                        Date = g.Key.ToDateTime(),
                        Aggregate = g.Min(e => Convert.ToDecimal(EF.Property<object>(e, propertyName)))
                    })
                    .OrderBy(r => r.Date)
                    .ToList(),

                AggregateFunction.Max => groupedQuery
                    .Select(g => new TrendValue
                    {
                        Date = g.Key.ToDateTime(),
                        Aggregate = g.Max(e => Convert.ToDecimal(EF.Property<object>(e, propertyName)))
                    })
                    .OrderBy(r => r.Date)
                    .ToList(),

                _ => throw new NotSupportedException($"Aggregation function '{function}' is not supported.")
            };
        }

        /// <summary>
        /// Fills in missing periods in the date range with zero values
        /// </summary>
        private List<TrendValue> FillMissingPeriods(List<TrendValue> results)
        {
            // Create a list to hold all possible periods in the range
            var allPeriods = new List<DateTime>();
            var currentPeriod = GetIntervalStart(_start);
            var endPeriod = GetIntervalStart(_end);

            // Generate all possible periods
            while (currentPeriod <= endPeriod)
            {
                allPeriods.Add(currentPeriod);
                currentPeriod = GetNextPeriod(currentPeriod);
            }

            // Convert existing results to a dictionary for easy lookup
            // Use a case-insensitive dictionary to avoid duplicate key issues
            var resultsByPeriod = new Dictionary<DateTime, TrendValue>();
            
            // Add each result to the dictionary, ensuring no duplicates
            foreach (var result in results)
            {
                if (!resultsByPeriod.ContainsKey(result.Date))
                {
                    resultsByPeriod.Add(result.Date, result);
                }
                else
                {
                    // If there's a duplicate date, add the aggregates together
                    var existingValue = resultsByPeriod[result.Date];
                    existingValue.Aggregate += result.Aggregate;
                }
            }

            // Create a list with all periods, using 0 for missing values
            var filledResults = new List<TrendValue>();
            foreach (var period in allPeriods)
            {
                if (resultsByPeriod.TryGetValue(period, out var result))
                {
                    filledResults.Add(result);
                }
                else
                {
                    filledResults.Add(new TrendValue
                    {
                        Date = period,
                        Aggregate = 0
                    });
                }
            }

            return filledResults;
        }

        /// <summary>
        /// Gets the start of an interval for a date
        /// </summary>
        private DateTime GetIntervalStart(DateTime date)
        {
            return _interval switch
            {
                "minute" => new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0),
                "hour" => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0),
                "day" => date.Date,
                "week" => GetStartOfWeek(date),
                "month" => new DateTime(date.Year, date.Month, 1),
                "year" => new DateTime(date.Year, 1, 1),
                _ => throw new NotSupportedException($"Interval '{_interval}' is not supported.")
            };
        }

        /// <summary>
        /// Gets the next period based on the interval
        /// </summary>
        private DateTime GetNextPeriod(DateTime date)
        {
            return _interval switch
            {
                "minute" => date.AddMinutes(1),
                "hour" => date.AddHours(1),
                "day" => date.AddDays(1),
                "week" => date.AddDays(7),
                "month" => date.AddMonths(1),
                "year" => date.AddYears(1),
                _ => throw new NotSupportedException($"Interval '{_interval}' is not supported.")
            };
        }

        /// <summary>
        /// Gets the start of the week containing the specified date
        /// </summary>
        private static DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}