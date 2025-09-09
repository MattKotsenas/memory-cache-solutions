using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CacheImplementations;

public static class MeterCacheExtensions
{
    public static IServiceCollection AddMemoryCacheViaDecorator(this IServiceCollection services, Action<MemoryCacheOptions2>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        configure ??= (_ => { });

        services.AddOptions();

        services.Configure(configure);
        services.ReMapMemoryCacheOptions();

        services.TryAdd(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());

        services.TryAddSingleton<IMemoryCache>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MemoryCacheOptions2>>();
            var inner = ActivatorUtilities.GetServiceOrCreateInstance<MemoryCache>(sp);

            if (options.Value.TrackStatistics)
            {
                var meter = new System.Diagnostics.Metrics.Meter(options.Value.Name);
                return new MeteredMemoryCache(inner, meter, disposeInner: true);
            }

            return inner;
        });

        return services;
    }

    public static IServiceCollection AddMemoryCacheViaObserver(this IServiceCollection services, Action<MemoryCacheOptions2>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        configure ??= (_ => { });

        services.AddOptions();
        services.Configure(configure);
        services.ReMapMemoryCacheOptions();

        services.TryAddSingleton<IMemoryCache>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MemoryCacheOptions2>>();

            var inner = ActivatorUtilities.GetServiceOrCreateInstance<MemoryCache>(sp);

            if (options.Value.TrackStatistics)
            {
                // Force the observer to start
                _ = new MemoryCacheObserver(inner, options.Value.Name);
            }

            return inner;
        });

        return services;
    }

    private static IServiceCollection ReMapMemoryCacheOptions(this IServiceCollection services)
    {
        // Don't look here. This is just because we've subclassed MemoryCacheOptions in the example.
        services.AddSingleton<IPostConfigureOptions<MemoryCacheOptions>>(sp =>
        {
            return new PostConfigureOptions<MemoryCacheOptions>(Options.DefaultName, baseOptions =>
            {
                var derived = sp.GetRequiredService<IOptions<MemoryCacheOptions2>>().Value;

                baseOptions.SizeLimit = derived.SizeLimit;
                baseOptions.CompactionPercentage = derived.CompactionPercentage;
                baseOptions.ExpirationScanFrequency = derived.ExpirationScanFrequency;
                baseOptions.TrackStatistics = derived.TrackStatistics;
                baseOptions.TrackLinkedCacheEntries = derived.TrackLinkedCacheEntries;
            });
        });

        return services;
    }
}
