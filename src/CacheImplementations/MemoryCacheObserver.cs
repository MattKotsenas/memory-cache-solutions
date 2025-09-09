using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace CacheImplementations;

public class MemoryCacheObserver
{
    private readonly IMemoryCache _cache;

    [SuppressMessage("Minor Code Smell", "S1450:Private fields only used as local variables in methods should become local variables", Justification = "Meter's lifetime should match that of observer.")]
    private readonly Meter _meter;

    // NOTE: Can't track evictions, but that also requires a callback per entry

    public MemoryCacheObserver(IMemoryCache cache, string name)
    {
        _cache = cache;
        _meter = new Meter("MyApp.Caching", "1.0", tags: [new KeyValuePair<string, object?>("name", name)]);

        _meter.CreateObservableGauge<long>("cache_entries", () =>
        {
            var stats = _cache.GetCurrentStatistics();
            return stats?.CurrentEntryCount ?? 0;
        });

        _meter.CreateObservableGauge<long>("cache_estimated_size", () =>
        {
            var stats = _cache.GetCurrentStatistics();
            return stats?.CurrentEstimatedSize ?? 0;
        });

        _meter.CreateObservableCounter<long>("cache_hits", () =>
        {
            var stats = _cache.GetCurrentStatistics();
            return stats?.TotalHits ?? 0;
        });

        _meter.CreateObservableCounter<long>("cache_misses", () =>
        {
            var stats = _cache.GetCurrentStatistics();
            return stats?.TotalMisses ?? 0;
        });
    }
}
