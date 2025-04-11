using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// Provides extension methods for Npgsql database functions.
    /// </summary>
    public static class NpgsqlDbFunctionsExtensions
    {
        /// <summary>
        /// Converts a DateTime value to a string representation based on the specified format.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance. This parameter is not used.</param>
        /// <param name="dateTime">The DateTime value to format.</param>
        /// <param name="format">The format string to use for conversion.</param>
        /// <returns>A string representation of the DateTime value in the specified format.</returns>
        /// <remarks>
        /// This method is intended for use with Entity Framework Core and should not be called directly.
        /// </remarks>
        public static string ToChar(this DbFunctions _, DateTime dateTime, string format)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
    }
}