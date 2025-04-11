using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// MySQL database adapter
    /// </summary>
    public class MySqlAdapter : IDatabaseAdapter
    {
        public Expression<Func<T, string>> FormatDate<T>(Expression<Func<T, DateTime>> dateProperty, string interval)
        {
            var paramExpr = dateProperty.Parameters[0];
            var dateExpr = dateProperty.Body;
            
            // Determine method and format based on interval
            string format = interval switch
            {
                "minute" => "%Y-%m-%d %H:%i:00",
                "hour" => "%Y-%m-%d %H:00",
                "day" => "%Y-%m-%d",
                "month" => "%Y-%m",
                "year" => "%Y",
                "week" => "%Y-%u", // MySQL week format
                _ => throw new NotSupportedException($"Interval '{interval}' is not supported.")
            };
            
            // Create expression for DATE_FORMAT(date, format)
            var formatMethod = typeof(MySqlDbFunctionsExtensions).GetMethod("DateFormat",
                new[] { typeof(DbFunctions), typeof(DateTime), typeof(string) }) ?? throw new InvalidOperationException("MySQL DateFormat function not found");
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