using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Options;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Extensions;

/// <summary>
/// Shared registration used by each domain package's own <c>AddRaptorSheets*</c> extension.
/// Domains call this rather than re-implementing the wiring.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a domain's manager factory, plus - when <paramref name="configureOptions"/> is
    /// supplied - a singleton manager bound to those options.
    /// <para>
    /// Options are registered as <em>named</em> options keyed by <paramref name="domainName"/>, so
    /// several domains can be registered side by side against different spreadsheets without
    /// overwriting each other's configuration.
    /// </para>
    /// </summary>
    /// <typeparam name="TManager">The domain's manager interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="domainName">Domain name, used as the named-options key and in error messages.</param>
    /// <param name="create">Builds the domain's manager from a sheet service and logger.</param>
    /// <param name="configureOptions">
    /// Binds a fixed spreadsheet and credentials. Omit for hosts that only create managers per
    /// request through <see cref="ISheetManagerFactory{TManager}"/>; resolving
    /// <typeparamref name="TManager"/> directly then throws, since there is nothing to bind it to.
    /// </param>
    public static IServiceCollection AddSheetManager<TManager>(
        this IServiceCollection services,
        string domainName,
        Func<IGoogleSheetService, ILogger?, TManager> create,
        Action<RaptorSheetsOptions>? configureOptions = null)
        where TManager : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(domainName);
        ArgumentNullException.ThrowIfNull(create);

        if (configureOptions is not null)
        {
            services.Configure(domainName, configureOptions);
        }

        services.AddSingleton<SheetManagerFactory<TManager>>(provider =>
        {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger($"RaptorSheets.{domainName}");

            return new SheetManagerFactory<TManager>(create, logger);
        });

        services.AddSingleton<ISheetManagerFactory<TManager>>(
            provider => provider.GetRequiredService<SheetManagerFactory<TManager>>());

        if (configureOptions is not null)
        {
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<RaptorSheetsOptions>>().Get(domainName);

                return provider.GetRequiredService<SheetManagerFactory<TManager>>()
                    .CreateFromOptions(options, domainName);
            });
        }

        return services;
    }
}
