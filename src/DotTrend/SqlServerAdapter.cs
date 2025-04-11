using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// SQL Server database adapter
    /// </summary>
    public class SqlServerAdapter : IDatabaseAdapter
    {
        /// <inheritdoc/>
        public Expression<Func<T, string>> FormatDate<T>(Expression<Func<T, DateTime>> dateProperty, string interval)
        {
            var paramExpr = dateProperty.Parameters[0];
            var dateExpr = dateProperty.Body;

            Expression<Func<T, string>> result;

            switch (interval)
            {
                case "minute":
                    result = Expression.Lambda<Func<T, string>>(
                        Expression.Call(
                            typeof(DbFunctionsExtensions),
                            nameof(DbFunctionsExtensions.DateTimeFormat),
                            null,
                            Expression.Constant(EF.Functions),
                            dateExpr,
                            Expression.Constant("yyyy-MM-dd HH:mm:00")
                        ),
                        paramExpr
                    );
                    break;
                case "hour":
                    result = Expression.Lambda<Func<T, string>>(
                        Expression.Call(
                            typeof(DbFunctionsExtensions),
                            nameof(DbFunctionsExtensions.DateTimeFormat),
                            null,
                            Expression.Constant(EF.Functions),
                            dateExpr,
                            Expression.Constant("yyyy-MM-dd HH:00")
                        ),
                        paramExpr
                    );
                    break;
                case "day":
                    result = Expression.Lambda<Func<T, string>>(
                        Expression.Call(
                            typeof(DbFunctionsExtensions),
                            nameof(DbFunctionsExtensions.DateTimeFormat),
                            null,
                            Expression.Constant(EF.Functions),
                            dateExpr,
                            Expression.Constant("yyyy-MM-dd")
                        ),
                        paramExpr
                    );
                    break;
                case "month":
                    result = Expression.Lambda<Func<T, string>>(
                        Expression.Call(
                            typeof(DbFunctionsExtensions),
                            nameof(DbFunctionsExtensions.DateTimeFormat),
                            null,
                            Expression.Constant(EF.Functions),
                            dateExpr,
                            Expression.Constant("yyyy-MM")
                        ),
                        paramExpr
                    );
                    break;
                case "year":
                    result = Expression.Lambda<Func<T, string>>(
                        Expression.Call(
                            typeof(DbFunctionsExtensions),
                            nameof(DbFunctionsExtensions.DateTimeFormat),
                            null,
                            Expression.Constant(EF.Functions),
                            dateExpr,
                            Expression.Constant("yyyy")
                        ),
                        paramExpr
                    );
                    break;
                case "week":
                    // For SQL Server, we need to use DatePart for the ISO week
                    var datePart = typeof(DbFunctionsExtensions).GetMethod("DatePart",
                        new[] { typeof(DbFunctions), typeof(string), typeof(DateTime) }) ?? throw new InvalidOperationException("DatePart method not found in DbFunctionsExtensions.");
                    var yearPart = Expression.Call(
                        typeof(DbFunctionsExtensions),
                        nameof(DbFunctionsExtensions.DateTimeFormat),
                        null,
                        Expression.Constant(EF.Functions),
                        dateExpr,
                        Expression.Constant("yyyy")
                    );

                    var weekPart = Expression.Call(
                        datePart,
                        Expression.Constant(EF.Functions),
                        Expression.Constant("isowk"),
                        dateExpr
                    );

                    result = Expression.Lambda<Func<T, string>>(
                        Expression.Call(
                            typeof(string),
                            nameof(string.Concat),
                            null,
                            yearPart,
                            Expression.Constant("-"),
                            Expression.Call(
                                typeof(string),
                                nameof(string.Format),
                                null,
                                Expression.Constant("{0:D2}"),
                                Expression.Convert(weekPart, typeof(object))
                            )
                        ),
                        paramExpr
                    );
                    break;
                default:
                    throw new NotSupportedException($"Interval '{interval}' is not supported.");
            }

            return result;
        }
    }
}