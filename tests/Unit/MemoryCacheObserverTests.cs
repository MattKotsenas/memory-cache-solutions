using CacheImplementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Unit;

public class MemoryCacheObserverTests
{
    [Fact]
    public void Foo()
    {
        var services = new ServiceCollection();
        services.AddKeyedMemoryCacheViaObserver(configure: o => o.TrackStatistics = true);
        var provider = services.BuildServiceProvider();

        var listener = new TestListener("cache_entries", "cache_estimated_size", "cache_hits", "cache_misses");

        var cache = provider.GetRequiredKeyedService<IMemoryCache>(Options.DefaultName);

        cache.TryGetValue("k", out _); // miss
        cache.Set("k", 10);            // set
        cache.TryGetValue("k", out _); // hit

        listener.RecordObservableInstruments();
    }
}
