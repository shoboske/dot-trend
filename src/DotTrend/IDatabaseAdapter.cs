using System.Linq.Expressions;

namespace DotTrend
{
    /// <summary>
    /// Interface for database-specific adapters that format dates for different database providers.
    /// Each database provider (SQL Server, MySQL, PostgreSQL, SQLite) has its own implementation
    /// with specific date formatting functions.
    /// </summary>
    public interface IDatabaseAdapter
    {
        /// <summary>
        /// Creates an expression that formats a date property according to the specified interval
        /// using database-specific date formatting functions.
        /// </summary>
        /// <typeparam name="T">The entity type containing the date property</typeparam>
        /// <param name="dateProperty">Lambda expression selecting the date property</param>
        /// <param name="interval">The time interval to format for (minute, hour, day, week, month, year)</param>
        /// <returns>An expression that formats the date as a string according to the interval</returns>
        Expression<Func<T, string>> FormatDate<T>(Expression<Func<T, DateTime>> dateProperty, string interval);
    }
}