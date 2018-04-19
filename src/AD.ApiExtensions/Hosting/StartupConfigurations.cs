using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

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
        /// <param name="commandLineArguments">
        /// Command line arguments to add to the <see cref="IConfiguration"/>.
        /// </param>
        /// <param name="optionalUserSecrets">
        /// True if configuration options from user secrets are optional; otherwise false.
        /// </param>
        /// <param name="optionalHostingFile">
        /// True if configuration options from hosting.json are optional; otherwise false.
        /// </param>
        /// <typeparam name="T">
        /// The <see cref="IStartup"/> to add to the <see cref="IWebHostBuilder"/>.
        /// </typeparam>
        /// <returns>
        /// The <see cref="IWebHostBuilder"/> with the <see cref="IConfiguration"/> and <see cref="IStartup"/> added.
        /// </returns>
        [Pure]
        [NotNull]
        public static IWebHostBuilder UseStartup<T>([NotNull] this IWebHostBuilder builder, StringValues commandLineArguments, bool optionalUserSecrets = true, bool optionalHostingFile = true) where T : class, IStartup
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            IConfiguration configuration =
                new ConfigurationBuilder()
                    .AddJsonFile("hosting.json", optionalHostingFile)
                    .AddUserSecrets<T>(optionalUserSecrets)
                    .AddCommandLine(commandLineArguments)
                    .Build();

            return
                builder.UseConfiguration(configuration)
                       .UseStartup<T>()
                       .UseDefaultServiceProvider((ctx, x) => x.ValidateScopes = ctx.HostingEnvironment.IsDevelopment());
        }
    }
}