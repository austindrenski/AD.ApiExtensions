using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AD.ApiExtensions.Conventions
{
    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public static class SnakeCaseEntityConvetions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="builder">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static ModelBuilder UseSnakeCase([NotNull] this ModelBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.Relational().TableName.CamelCaseToSnakeCase();

                foreach (IMutableProperty property in entity.GetProperties())
                {
                    property.Relational().ColumnName = property.Name.CamelCaseToSnakeCase();
                }

                foreach (IMutableKey key in entity.GetKeys())
                {
                    key.Relational().Name = key.Relational().Name.CamelCaseToSnakeCase();
                }

                foreach (IMutableForeignKey key in entity.GetForeignKeys())
                {
                    key.Relational().Name = key.Relational().Name.CamelCaseToSnakeCase();
                }

                foreach (IMutableIndex index in entity.GetIndexes())
                {
                    index.Relational().Name = index.Relational().Name.CamelCaseToSnakeCase();
                }
            }

            return builder;
        }
    }
}