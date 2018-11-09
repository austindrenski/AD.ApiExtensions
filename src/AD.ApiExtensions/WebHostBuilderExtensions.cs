using System;
using System.Reflection;
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
        /// <remarks>
        ///   <para>
        ///   The <see cref="IConfiguration"/> is populated from:
        ///   <list type="number">
        ///     <item>
        ///       <description>
        ///         The initial <see cref="IWebHostBuilder"/>.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         An optional appsettings.json file.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         An optional appsettings.{EnvironmentName}.json file
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         The commandline arguments.
        ///       </description>
        ///     </item>
        ///   </list>
        ///   </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/></exception>
        [Pure]
        [NotNull]
        public static IWebHostBuilder UseStartup<T>([NotNull] this IWebHostBuilder builder, [NotNull] string[] args)
            where T : class, IStartup
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            AssemblyName info = typeof(T).Assembly.GetName();

            return
                builder.UseSetting(WebHostDefaults.ApplicationKey, info.Name)
                       .UseSetting("applicationVersion", info.Version.ToString())
                       .ConfigureAppConfiguration(
                           (ctx, config) =>
                               config.AddJsonFile("appsettings.json", true)
                                     .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", true)
                                     .AddCommandLine(args))
                       .ConfigureServices(x => x.AddSingleton(typeof(IStartup), typeof(T)));
        }
    }
}