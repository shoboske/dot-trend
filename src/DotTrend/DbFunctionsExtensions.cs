using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// Provides extension methods for <see cref="DbFunctions"/> to support database-specific operations.
    /// </summary>
    public static class DbFunctionsExtensions
    {
        /// <summary>
        /// Formats a <see cref="DateTime"/> value as a string using the specified format.
        /// </summary>
        /// <param name="dbFunctions">The <see cref="DbFunctions"/> instance. This parameter is not used.</param>
        /// <param name="dateTime">The <see cref="DateTime"/> value to format.</param>
        /// <param name="format">The format string to apply.</param>
        /// <returns>A string representation of the <see cref="DateTime"/> value in the specified format.</returns>
        /// <remarks>
        /// This method is intended for use with Entity Framework Core and should not be called directly.
        /// </remarks>
        public static string DateTimeFormat(this DbFunctions dbFunctions, DateTime dateTime, string format)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }

        /// <summary>
        /// Extracts a specific part of a <see cref="DateTime"/> value, such as the year, month, or day.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance. This parameter is not used.</param>
        /// <param name="datePartArg">The part of the date to extract (e.g., "year", "month", "day").</param>
        /// <param name="dateTime">The <see cref="DateTime"/> value to extract the part from.</param>
        /// <returns>An integer representing the extracted part of the <see cref="DateTime"/> value.</returns>
        /// <remarks>
        /// This method is intended for use with Entity Framework Core and should not be called directly.
        /// </remarks>
        public static int DatePart(this DbFunctions _, string datePartArg, DateTime dateTime)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
    }
}