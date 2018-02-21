using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <inheritdoc />
    /// <summary>
    /// Defines access properties to a logging database.
    /// </summary>
    [PublicAPI]
    public interface ILoggingContext : IDisposable
    {
        /// <summary>
        /// Identifies the current session for logging.
        /// </summary>
        Guid SessionId { get; }

        /// <summary>
        /// Provides read access to the event log entities.
        /// </summary>
        IQueryable<ILogEntry> LogEntries { get; }

        /// <summary>
        /// Adds an entry to the log.
        /// </summary>
        /// <param name="entry">
        /// The entry to be added.
        /// </param>
        void AddLogEntry<TEntry>([NotNull] TEntry entry) where TEntry : class, ILogEntry;

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateException" />
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateConcurrencyException" />
        /// <exception cref="T:System.Data.Entity.Validation.DbEntityValidationException" />
        /// <exception cref="T:System.NotSupportedException" />
        /// <exception cref="T:System.ObjectDisposedException" />
        /// <exception cref="T:System.InvalidOperationException" />
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateException"/>
        /// <exception cref="T:System.Data.Entity.Infrastructure.DbUpdateConcurrencyException"/>
        /// <exception cref="T:System.Data.Entity.Validation.DbEntityValidationException"/>
        /// <exception cref="T:System.NotSupportedException"/>
        /// <exception cref="T:System.ObjectDisposedException"/>
        /// <exception cref="T:System.InvalidOperationException"/>
        int SaveChanges();
    }
}