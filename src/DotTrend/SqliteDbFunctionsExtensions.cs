using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    public static class SqliteDbFunctionsExtensions
    {
        public static string Strftime(this DbFunctions _, string format, DateTime dateTime)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
    }
}