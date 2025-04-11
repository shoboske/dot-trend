using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// PostgreSQL database adapter
    /// </summary>
    public class PostgresAdapter : IDatabaseAdapter
    {
        /// <inheritdoc/>
        public Expression<Func<T, string>> FormatDate<T>(Expression<Func<T, DateTime>> dateProperty, string interval)
        {
            var paramExpr = dateProperty.Parameters[0];
            var dateExpr = dateProperty.Body;

            // Determine method and format based on interval
            string format = interval switch
            {
                "minute" => "YYYY-MM-DD HH24:MI:00",
                "hour" => "YYYY-MM-DD HH24:00",
                "day" => "YYYY-MM-DD",
                "month" => "YYYY-MM",
                "year" => "YYYY",
                "week" => "YYYY-IW", // PostgreSQL ISO week format
                _ => throw new NotSupportedException($"Interval '{interval}' is not supported.")
            };

            // Create expression for TO_CHAR(date, format)
            var formatMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod("ToChar",
                new[] { typeof(DbFunctions), typeof(DateTime), typeof(string) }) ?? throw new InvalidOperationException("PostgreSQL ToChar function not found");
            var formatCall = Expression.Call(
                formatMethod,
                Expression.Constant(EF.Functions),
                dateExpr,
                Expression.Constant(format)
            );

            return Expression.Lambda<Func<T, string>>(formatCall, paramExpr);
        }
    }
}