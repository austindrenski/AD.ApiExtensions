using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AD.ApiExtensions.Hosting
{
    /// <summary>
    /// Provides extensions to configure <see cref="IStartup"/> instances.
    /// </summary>
    [PublicAPI]
    public static class StartupConfigurations
    {
        /// <summary>
        /// Builds an <see cref="IConfiguration"/> and adds <typeparamref name="T"/> as an <see cref="IStartup"/> to the <see cref="IWebHostBuilder"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder to modify.
        /// </param>
        /// <param name="args">Command line arguments to add to the <see cref="IConfiguration"/>.</param>
        /// <typeparam name="T">The <see cref="IStartup"/> to add to the <see cref="IWebHostBuilder"/>.</typeparam>
        /// <returns>
        /// The <see cref="IWebHostBuilder"/> with the <see cref="IConfiguration"/> and <see cref="IStartup"/> added.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/></exception>
        [Pure]
        [NotNull]
        public static IWebHostBuilder UseStartup<T>([NotNull] this IWebHostBuilder builder, [NotNull] string[] args) where T : class, IStartup
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            IConfiguration configuration =
                new ConfigurationBuilder()
                   .AddCommandLine(args)
                   .Build();

            return
                builder.UseConfiguration(configuration)
                       .UseStartup<T>()
                       .UseDefaultServiceProvider((ctx, x) => x.ValidateScopes = ctx.HostingEnvironment.IsDevelopment());
        }
    }
}