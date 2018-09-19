using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Provides extensions to configure <see cref="IWebHostBuilder"/>.
    /// </summary>
    [PublicAPI]
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Builds an <see cref="IConfiguration"/> and adds <typeparamref name="T"/> as an
        /// <see cref="IStartup"/> to the <see cref="IWebHostBuilder"/>.
        /// </summary>
        /// <param name="builder">The builder to modify.</param>
        /// <param name="args">Command line arguments to add to the <see cref="IConfiguration"/>.</param>
        /// <typeparam name="T">The <see cref="IStartup"/> to add to the <see cref="IWebHostBuilder"/>.</typeparam>
        /// <returns>
        /// The <see cref="IWebHostBuilder"/> with the <see cref="IConfiguration"/> and <see cref="IStartup"/> added.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/></exception>
        [Pure]
        [NotNull]
        public static IWebHostBuilder UseStartup<T>([NotNull] this IWebHostBuilder builder, [NotNull] string[] args)
            where T : class, IStartup
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return
                builder.UseSetting(WebHostDefaults.ApplicationKey, typeof(T).Assembly.GetName().Name)
                       .UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build())
                       .ConfigureServices(x => x.AddSingleton(typeof(IStartup), typeof(T)));
        }
    }
}