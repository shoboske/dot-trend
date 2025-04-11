using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// Provides extension methods for SQLite database functions in Entity Framework Core.
    /// </summary>
    public static class SqliteDbFunctionsExtensions
    {
        /// <summary>
        /// Formats a date and time value according to the specified format string.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance. This parameter is not used.</param>
        /// <param name="format">The format string to apply to the date and time value.</param>
        /// <param name="dateTime">The date and time value to format.</param>
        /// <returns>A string representation of the formatted date and time.</returns>
        public static string Strftime(this DbFunctions _, string format, DateTime dateTime)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
    }
}