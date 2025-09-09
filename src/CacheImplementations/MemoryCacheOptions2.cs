using Microsoft.Extensions.Caching.Memory;

namespace CacheImplementations;

public class MemoryCacheOptions2 : MemoryCacheOptions
{
    // Would add to base class here rather than extend; just for prototyping
    public string Name { get; set; } = "Default"; // Could probably also be null
}
