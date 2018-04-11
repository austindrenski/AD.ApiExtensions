using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AD.ApiExtensions.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods to apply snake case conventions.
    /// </summary>
    [PublicAPI]
    public static class UseSnakeCaseExtensions
    {
        /// <summary>
        /// Applies snake case conventions for table names, columns, keys, foreign keys, and indexes.
        /// This method follows the ASP.NET Core convention of returning the original reference with modifications (i.e. not pure).
        /// </summary>
        /// <param name="builder">
        /// The builder to modify.
        /// </param>
        /// <param name="overwrite">
        /// True if a non-default (e.g. previously set) table name should be overwritten; otherwise false.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public static ModelBuilder UseSnakeCase([NotNull] this ModelBuilder builder, bool overwrite = false)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
            {
                entity.UseSnakeCase(overwrite);
            }

            return builder;
        }

        /// <summary>
        /// Applies snake case convention to the table name of the entity.
        /// This method follows the ASP.NET Core convention of returning the original reference with modifications (i.e. not pure).
        /// </summary>
        /// <param name="entity">
        /// The entity to modify.
        /// </param>
        /// <param name="overwrite">
        /// True if a non-default (e.g. previously set) table name should be overwritten; otherwise false.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public static IMutableEntityType UseSnakeCase([NotNull] this IMutableEntityType entity, bool overwrite = false)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            RelationalEntityTypeAnnotations entityTypeAnnotations = entity.Relational();

            if (overwrite || entityTypeAnnotations.TableName == entity.ClrType?.Name)
            {
                entityTypeAnnotations.TableName = entityTypeAnnotations.TableName?.CamelCaseToSnakeCase();
            }
            else
            {
                // If the name is assigned from DbSet, then it needs to be normalized.
                entityTypeAnnotations.TableName = entityTypeAnnotations.TableName?.ToLower();
            }

            foreach (IMutableProperty property in entity.GetProperties())
            {
                property.UseSnakeCase();
            }

            foreach (IMutableKey key in entity.GetKeys())
            {
                key.UseSnakeCase();
            }

            foreach (IMutableForeignKey key in entity.GetForeignKeys())
            {
                key.UseSnakeCase();
            }

            foreach (IMutableIndex index in entity.GetIndexes())
            {
                index.UseSnakeCase();
            }

            return entity;
        }

        /// <summary>
        /// Applies the snake case convention to the column name of the property.
        /// This method follows the ASP.NET Core convention of returning the original reference with modifications (i.e. not pure).
        /// </summary>
        /// <param name="property">
        /// The property to modify.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public static IMutableProperty UseSnakeCase([NotNull] this IMutableProperty property)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            RelationalPropertyAnnotations propertyAnnotations = property.Relational();

            if (propertyAnnotations.ColumnName == property.Name)
            {
                propertyAnnotations.ColumnName = propertyAnnotations.ColumnName.CamelCaseToSnakeCase();
            }

            return property;
        }

        /// <summary>
        /// Applies the snake case convention to the key.
        /// This method follows the ASP.NET Core convention of returning the original reference with modifications (i.e. not pure).
        /// </summary>
        /// <param name="key">
        /// The key to modify.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public static IMutableKey UseSnakeCase([NotNull] this IMutableKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            key.Relational().Name = key.Relational().Name.CamelCaseToSnakeCase();


            return key;
        }

        /// <summary>
        /// Applies the snake case convention to the foreign key.
        /// This method follows the ASP.NET Core convention of returning the original reference with modifications (i.e. not pure).
        /// </summary>
        /// <param name="key">
        /// The foreign key to modify.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public static IMutableForeignKey UseSnakeCase([NotNull] this IMutableForeignKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            key.Relational().Name = key.Relational().Name.CamelCaseToSnakeCase();


            return key;
        }

        /// <summary>
        /// Applies the snake case convention to the index.
        /// This method follows the ASP.NET Core convention of returning the original reference with modifications (i.e. not pure).
        /// </summary>
        /// <param name="index">
        /// The index to modify.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public static IMutableIndex UseSnakeCase([NotNull] this IMutableIndex index)
        {
            if (index is null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            index.Relational().Name = index.Relational().Name.CamelCaseToSnakeCase();

            return index;
        }
    }
}