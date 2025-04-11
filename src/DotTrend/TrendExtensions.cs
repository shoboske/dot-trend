using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DotTrend
{
    /// <summary>
    /// Extension methods for using DotTrend with DbContext and IQueryable.
    /// </summary>
    public static class TrendExtensions
    {
        /// <summary>
        /// Creates a new trend analysis for the given IQueryable.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="query">The IQueryable source</param>
        /// <param name="databaseAdapter">Optional database adapter (defaults to SQL Server)</param>
        /// <returns>A new Trend instance for this query</returns>
        public static Trend<T> Trend<T>(this IQueryable<T> query, IDatabaseAdapter? databaseAdapter = null) where T : class
        {
            return DotTrend.Trend<T>.Query(query, databaseAdapter);
        }

        /// <summary>
        /// Creates a new trend analysis for the given DbSet.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="dbSet">The DbSet source</param>
        /// <param name="databaseAdapter">Optional database adapter (defaults to SQL Server)</param>
        /// <returns>A new Trend instance for this DbSet</returns>
        public static Trend<T> Trend<T>(this DbSet<T> dbSet, IDatabaseAdapter? databaseAdapter = null) where T : class
        {
            return DotTrend.Trend<T>.Query(dbSet, databaseAdapter);
        }

        /// <summary>
        /// Enables easy access to the appropriate database adapter for a DbContext
        /// </summary>
        /// <param name="context">The DbContext</param>
        /// <returns>A database adapter matching the provider for this context</returns>
        public static IDatabaseAdapter GetTrendAdapter(this DbContext context)
        {
            var providerName = context.Database.ProviderName?.ToLowerInvariant() ?? "";

            return providerName switch
            {
                var n when n.Contains("sqlserver") => new SqlServerAdapter(),
                var n when n.Contains("mysql") => new MySqlAdapter(),
                var n when n.Contains("sqlite") => new SqliteAdapter(),
                var n when n.Contains("postgresql") || n.Contains("npgsql") => new PostgresAdapter(),
                _ => new SqlServerAdapter() // Default to SQL Server
            };
        }

        /// <summary>
        /// Configures the DotTrend library to use this DbContext instance for static methods
        /// </summary>
        /// <param name="context">The DbContext to use</param>
        public static void UseTrend(this DbContext context)
        {
            TrendDbContextProvider.SetGlobalDbContext(context);
        }
    }
}