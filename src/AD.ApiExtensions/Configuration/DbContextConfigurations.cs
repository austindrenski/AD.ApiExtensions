using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AD.ApiExtensions.Configuration
{
    /// <summary>
    /// Provides extensions to configure <see cref="DbContext"/> instances.
    /// </summary>
    [PublicAPI]
    public static class DbContextConfigurations
    {
        /// <summary>
        /// Configures the <see cref="DbContext"/> type as a PostgreSQL database with default configurations.
        /// This method follows the ASP.NET Core convention of returning the original reference with modifications (i.e. not pure).
        /// </summary>
        /// <param name="services">
        /// The service collection to modify.
        /// </param>
        /// <param name="connectionString">
        /// The connection string to use for the database.
        /// </param>
        /// <param name="queryTrackingBehavior">
        /// Sets the tracking behavior for LINQ queries run against the context.
        /// </param>
        /// <param name="npgsqlOptions">
        /// Configures the <see cref="Npgsql"/> provider.
        /// </param>
        /// <typeparam name="T">
        /// The type of the <see cref="DbContext"/>.
        /// </typeparam>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public static IServiceCollection AddPostgres<T>(
            [NotNull] this IServiceCollection services,
            [NotNull] string connectionString,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.NoTracking,
            [CanBeNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptions = default) where T : DbContext
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (connectionString is null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return
                services.AddEntityFrameworkNpgsql()
                        .AddDbContext<T>(
                            x =>
                            {
                                x.UseQueryTrackingBehavior(queryTrackingBehavior);
                                x.UseNpgsql(
                                    connectionString,
                                    y =>
                                    {
                                        if (npgsqlOptions is null)
                                        {
                                            y.EnableRetryOnFailure();
                                            y.UseRelationalNulls();
                                        }
                                        else
                                        {
                                            npgsqlOptions(y);
                                        }
                                    });
                            });
        }
    }
}