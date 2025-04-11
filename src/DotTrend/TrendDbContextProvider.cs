using Microsoft.EntityFrameworkCore;

namespace DotTrend
{
    /// <summary>
    /// Provides DbContext access for the Trend library when using static methods.
    /// </summary>
    public static class TrendDbContextProvider
    {
        private static Func<Type, object>? _dbContextFactory;
        private static object? _globalDbContext;

        /// <summary>
        /// Sets a factory function that can create or retrieve a DbContext for a given entity type
        /// </summary>
        /// <param name="factory">A function that takes an entity Type and returns a DbContext</param>
        public static void SetDbContextFactory(Func<Type, object> factory)
        {
            _dbContextFactory = factory;
        }

        /// <summary>
        /// Sets a global DbContext instance to be used by all Trend operations
        /// </summary>
        /// <param name="dbContext">The DbContext to use for all operations</param>
        public static void SetGlobalDbContext(object dbContext)
        {
            _globalDbContext = dbContext;
        }

        /// <summary>
        /// Gets the DbSet for a given entity type from the configured DbContext
        /// </summary>
        /// <typeparam name="TEntity">The entity type to get a DbSet for</typeparam>
        /// <returns>The DbSet of the requested entity type, or null if none is found</returns>
        public static IQueryable<TEntity>? GetDbSet<TEntity>() where TEntity : class
        {
            // First try using the factory if available
            if (_dbContextFactory != null)
            {
                var context = _dbContextFactory(typeof(TEntity));
                if (context != null)
                {
                    var dbSetProperty = context.GetType().GetProperties()
                        .FirstOrDefault(p => p.PropertyType.IsGenericType
                                          && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                                          && p.PropertyType.GetGenericArguments()[0] == typeof(TEntity));

                    if (dbSetProperty != null)
                    {
                        return dbSetProperty.GetValue(context) as IQueryable<TEntity>;
                    }
                }
            }

            // Fall back to global context if available
            if (_globalDbContext != null)
            {
                var dbSetProperty = _globalDbContext.GetType().GetProperties()
                    .FirstOrDefault(p => p.PropertyType.IsGenericType
                                      && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                                      && p.PropertyType.GetGenericArguments()[0] == typeof(TEntity));

                if (dbSetProperty != null)
                {
                    return dbSetProperty.GetValue(_globalDbContext) as IQueryable<TEntity>;
                }
            }

            return null;
        }
    }
}