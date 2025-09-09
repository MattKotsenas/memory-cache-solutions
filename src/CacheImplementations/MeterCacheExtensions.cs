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

        services.Configure(configure); // What to do here; Name should go on Options so it isn't a breaking change, but then how do we do named options, which requires the name up front?
        services.Configure<MemoryCacheOptions>((o) => o.TrackStatistics = true); // This is just for testing, would come from the outer configure

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
        services.Configure(configure); // What to do here; Name should go on Options so it isn't a breaking change, but then how do we do named options, which requires the name up front?
        services.Configure<MemoryCacheOptions>((o) => o.TrackStatistics = true); // This is just for testing, would come from the outer configure

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
}
