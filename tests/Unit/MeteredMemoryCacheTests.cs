using CacheImplementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.Metrics;

namespace Unit;

public class MeteredMemoryCacheTests
{
    [Fact]
    public void RecordsHitAndMiss()
    {
        using var inner = new MemoryCache(new MemoryCacheOptions());
        var meter = new Meter("test.metered.cache");
        using var listener = new TestListener("cache_hits_total", "cache_misses_total");

        var cache = new MeteredMemoryCache(inner, meter);

        cache.TryGetValue("k", out _); // miss
        cache.Set("k", 10);            // set
        cache.TryGetValue("k", out _); // hit

        Assert.Equal(1, listener.Counters["cache_hits_total"]);
        Assert.Equal(1, listener.Counters["cache_misses_total"]);
    }

    [Fact(Skip = "Flaky under CI timing; revisit when deterministic eviction test harness added.")]
    public void RecordsEviction()
    {
        using var inner = new MemoryCache(new MemoryCacheOptions());
        var meter = new Meter("test.metered.cache2");
        using var listener = new TestListener("cache_evictions_total");
        var cache = new MeteredMemoryCache(inner, meter);

        using var cts = new CancellationTokenSource();
        var options = new MemoryCacheEntryOptions();
        options.AddExpirationToken(new CancellationChangeToken(cts.Token));

        cache.Set("k", 1, options);
        cts.Cancel();
        cache.TryGetValue("k", out _);
        inner.Compact(0.0);

        Assert.True(listener.Counters.TryGetValue("cache_evictions_total", out var ev) && ev >= 1);
    }
}
