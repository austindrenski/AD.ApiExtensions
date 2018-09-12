using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Provides extension methods to apply snake case conventions.
    /// </summary>
    [PublicAPI]
    public static class UseSnakeCaseExtensions
    {
        /// <summary>
        /// Applies snake case conventions to relational annotations for
        /// table names, columns, keys, foreign keys, and indexes.
        /// </summary>
        /// <param name="builder">The <see cref="ModelBuilder"/> to mutate.</param>
        /// <returns>
        /// The mutated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/></exception>
        [NotNull]
        public static ModelBuilder UseSnakeCase([NotNull] this ModelBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Model.UseSnakeCase();

            return builder;
        }

        /// <summary>
        /// Applies snake case conventions to relational annotations for
        /// table names, columns, keys, foreign keys, and indexes.
        /// </summary>
        /// <param name="annotatable">The annotatable to mutate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        [NotNull]
        public static IMutableAnnotatable UseSnakeCase([NotNull] this IMutableAnnotatable annotatable)
        {
            if (annotatable == null)
                throw new ArgumentNullException(nameof(annotatable));

            switch (annotatable)
            {
                case IMutableModel m:
                    return MutateModel(m);

                case IMutableEntityType e:
                    e.Relational().TableName = e.Relational().TableName.ConvertToSnakeCase();
                    return e;

                case IMutableForeignKey f:
                    f.Relational().Name = f.Relational().Name.ConvertToSnakeCase();
                    return f;

                case IMutableIndex i:
                    i.Relational().Name = i.Relational().Name.ConvertToSnakeCase();
                    return i;

                case IMutableKey k:
                    k.Relational().Name = k.Relational().Name.ConvertToSnakeCase();
                    return k;

                case IMutableProperty p:
                    p.Relational().ColumnName = p.Relational().ColumnName.ConvertToSnakeCase();
                    return p;

                case IMutableNavigation _:
                case IMutableServiceProperty _:
                case IMutablePropertyBase _:
                case IMutableTypeBase _:
                default:
                    return annotatable;
            }

            // Mutates the model in place to avoid recursive calls.
            IMutableModel MutateModel(IMutableModel m)
            {
                foreach (IMutableEntityType e in m.GetEntityTypes())
                {
                    e.UseSnakeCase();

                    foreach (IMutableForeignKey f in e.GetForeignKeys())
                        f.UseSnakeCase();

                    foreach (IMutableIndex i in e.GetIndexes())
                        i.UseSnakeCase();

                    foreach (IMutableKey k in e.GetKeys())
                        k.UseSnakeCase();

                    foreach (IMutableProperty p in e.GetProperties())
                        p.UseSnakeCase();
                }

                return m;
            }
        }
    }
}