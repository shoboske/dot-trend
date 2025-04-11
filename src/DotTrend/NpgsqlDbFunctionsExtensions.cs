using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    public static class NpgsqlDbFunctionsExtensions
    {
        public static string ToChar(this DbFunctions _, DateTime dateTime, string format)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and should not be called directly.");
        }
    }
}