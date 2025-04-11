using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    // Extension classes for database-specific functions

    public static class DbFunctionsExtensions
    {
        public static string DateTimeFormat(this DbFunctions _, DateTime dateTime, string format)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
        
        public static int DatePart(this DbFunctions _, string datePartArg, DateTime dateTime)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
    }
}