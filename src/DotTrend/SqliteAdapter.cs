using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// SQLite database adapter
    /// </summary>
    public class SqliteAdapter : IDatabaseAdapter
    {
        /// <inheritdoc/>
        public Expression<Func<T, string>> FormatDate<T>(Expression<Func<T, DateTime>> dateProperty, string interval)
        {
            var paramExpr = dateProperty.Parameters[0];
            var dateExpr = dateProperty.Body;

            // Determine method and format based on interval
            string format = interval switch
            {
                "minute" => "%Y-%m-%d %H:%M:00",
                "hour" => "%Y-%m-%d %H:00",
                "day" => "%Y-%m-%d",
                "month" => "%Y-%m",
                "year" => "%Y",
                "week" => "%Y-%W", // SQLite week format
                _ => throw new NotSupportedException($"Interval '{interval}' is not supported.")
            };

            // Create expression for strftime(format, date)
            var formatMethod = typeof(SqliteDbFunctionsExtensions).GetMethod("Strftime",
                new[] { typeof(DbFunctions), typeof(string), typeof(DateTime) }) ?? throw new InvalidOperationException("SQLite Strftime function not found");
            var formatCall = Expression.Call(
                formatMethod,
                Expression.Constant(EF.Functions),
                Expression.Constant(format),
                dateExpr
            );

            return Expression.Lambda<Func<T, string>>(formatCall, paramExpr);
        }
    }
}