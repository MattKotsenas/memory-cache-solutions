using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;

namespace CacheImplementations;

public static class MeterCacheExtensions
{
    // NOTE: In both cases we would have both a regular and a keyed registration; just doing the keyed since it's the more complex case.


    public static IServiceCollection AddKeyedMemoryCacheViaDecorator(this IServiceCollection services, string? name = null, Action<MemoryCacheOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        configure ??= (_ => { });
        name ??= Options.DefaultName;

        services.AddOptions();

        services.Configure(configure);

        services.TryAddKeyedSingleton<IMemoryCache>(name, (sp, key) =>
        {
            var options = sp.GetRequiredService<IOptions<MemoryCacheOptions>>();
            var inner = ActivatorUtilities.GetServiceOrCreateInstance<MemoryCache>(sp);

            if (options.Value.TrackStatistics)
            {
                var meter = new Meter(name);
                return new MeteredMemoryCache(inner, meter, disposeInner: true);
            }

            return inner;
        });

        return services;
    }

    public static IServiceCollection AddKeyedMemoryCacheViaObserver(this IServiceCollection services, string? name = null, Action<MemoryCacheOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        configure ??= (_ => { });
        name ??= Options.DefaultName;

        services.AddOptions();
        services.Configure(configure);

        services.TryAddKeyedSingleton<IMemoryCache>(name, (sp, key) =>
        {
            var options = sp.GetRequiredService<IOptions<MemoryCacheOptions>>();

            var inner = ActivatorUtilities.GetServiceOrCreateInstance<MemoryCache>(sp);

            if (options.Value.TrackStatistics)
            {
                // Force the observer to start
                _ = new MemoryCacheObserver(inner, name);
            }

            return inner;
        });

        return services;
    }
}
