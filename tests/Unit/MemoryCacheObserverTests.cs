using CacheImplementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Unit;

public class MemoryCacheObserverTests
{
    [Fact]
    public void DirectRegistration()
    {
        var services = new ServiceCollection();
        services.AddKeyedMemoryCacheViaObserver(name: "MyCache", configure: o => o.TrackStatistics = true);

        var provider = services.BuildServiceProvider();

        var listener = new TestListener("cache_entries", "cache_estimated_size", "cache_hits", "cache_misses");

        var cache = provider.GetRequiredKeyedService<IMemoryCache>("MyCache");

        cache.TryGetValue("k", out _); // miss
        cache.Set("k", 10);            // set
        cache.TryGetValue("k", out _); // hit

        listener.RecordObservableInstruments();
    }

    [Fact]
    public void AddingTracking()
    {
        var services = new ServiceCollection();
        services.AddKeyedMemoryCacheViaObserver(name: "MyCache"); // Assume registered via some other method like .AddMvc()

        services.Configure<MemoryCacheOptions>("MyCache", o => o.TrackStatistics = true);

        var provider = services.BuildServiceProvider();

        var listener = new TestListener("cache_entries", "cache_estimated_size", "cache_hits", "cache_misses");

        var cache = provider.GetRequiredKeyedService<IMemoryCache>("MyCache");

        cache.TryGetValue("k", out _); // miss
        cache.Set("k", 10);            // set
        cache.TryGetValue("k", out _); // hit

        listener.RecordObservableInstruments();
    }
}
