using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace AD.ApiExtensions.EntityFrameworkCore
{
    /// <summary>
    /// Provides extensions to configure <see cref="DbContext"/> instances.
    /// </summary>
    [PublicAPI]
    public static class DbContextConfigurations
    {
        /// <summary>
        /// Configures the <see cref="DbContext"/> type as a PostgreSQL database with:
        /// 1) sets change tracking to <see cref="QueryTrackingBehavior.TrackAll"/>;
        /// 2) enables retry on connection failure;
        /// 3) configures relational database semantics for null.
        /// </summary>
        /// <param name="services">
        /// The service collection to modify.
        /// </param>
        /// <param name="connectionString">
        /// The connection string to use for the database.
        /// </param>
        /// <typeparam name="T">
        /// The type of the <see cref="DbContext"/>.
        /// </typeparam>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public static IServiceCollection AddPostgres<T>([NotNull] this IServiceCollection services, [NotNull] string connectionString) where T : DbContext
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (connectionString is null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return services.AddPostgres<T>(connectionString, QueryTrackingBehavior.TrackAll);
        }

        /// <summary>
        /// Configures the <see cref="DbContext"/> type as a PostgreSQL database with:
        /// 1) enables retry on connection failure;
        /// 2) configures relational database semantics for null.
        /// </summary>
        /// <param name="services">
        /// The service collection to modify.
        /// </param>
        /// <param name="connectionString">
        /// The connection string to use for the database.
        /// </param>
        /// <param name="changeTracking">
        /// Sets the change tracking behavior for LINQ queries run against the context.
        /// </param>
        /// <typeparam name="T">
        /// The type of the <see cref="DbContext"/>.
        /// </typeparam>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public static IServiceCollection AddPostgres<T>([NotNull] this IServiceCollection services, [NotNull] string connectionString, QueryTrackingBehavior changeTracking) where T : DbContext
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (connectionString is null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return services.AddPostgres<T>(connectionString, changeTracking, x => x.EnableRetryOnFailure().UseRelationalNulls());
        }

        /// <summary>
        /// Configures the <see cref="DbContext"/> type as a PostgreSQL database.
        /// </summary>
        /// <param name="services">
        /// The service collection to modify.
        /// </param>
        /// <param name="connectionString">
        /// The connection string to use for the database.
        /// </param>
        /// <param name="changeTracking">
        /// Sets the change tracking behavior for LINQ queries run against the context.
        /// </param>
        /// <param name="npgsqlOptions">
        /// Configures the <see cref="Npgsql"/> provider.
        /// </param>
        /// <typeparam name="T">
        /// The type of the <see cref="DbContext"/>.
        /// </typeparam>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public static IServiceCollection AddPostgres<T>([NotNull] this IServiceCollection services, [NotNull] string connectionString, QueryTrackingBehavior changeTracking, [NotNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptions) where T : DbContext
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (connectionString is null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (npgsqlOptions is null)
            {
                throw new ArgumentNullException(nameof(npgsqlOptions));
            }

            return
                services.AddEntityFrameworkNpgsql()
                        .AddDbContext<T>(
                            x =>
                            {
                                x.UseQueryTrackingBehavior(changeTracking);
                                x.UseNpgsql(connectionString, npgsqlOptions);
                            });
        }
    }
}