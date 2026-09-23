using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Team3.Backend.Services.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedValue = await _cache.GetStringAsync(key);

        if (cachedValue is null)
            return default;

        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration)
    {
        var serializedValue = JsonSerializer.Serialize(value);

        await _cache.SetStringAsync(
            key,
            serializedValue,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
    }

    public Task RemoveAsync(string key)
    {
        return _cache.RemoveAsync(key);
    }
}