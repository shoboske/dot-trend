using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// Provides extension methods for MySQL database functions.
    /// </summary>
    public static class MySqlDbFunctionsExtensions
    {
        /// <summary>
        /// Formats a date using the specified format string in a MySQL-compatible manner.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance. This parameter is not used.</param>
        /// <param name="dateTime">The date and time to format.</param>
        /// <param name="format">The format string to apply.</param>
        /// <returns>A formatted date string.</returns>
        public static string DateFormat(this DbFunctions _, DateTime dateTime, string format)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
    }
}