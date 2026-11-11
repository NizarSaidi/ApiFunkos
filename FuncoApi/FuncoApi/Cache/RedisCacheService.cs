using System.Text.Json;
using FuncoApi.Cache.objet;
using Microsoft.Extensions.Caching.Distributed;

namespace FuncoApi.Cache;

/// <summary>Implementa <see cref="ICacheService"/> serializando valores sobre <see cref="IDistributedCache"/>.</summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    /// <summary>Crea el adaptador para una caché distribuida.</summary>
    public RedisCacheService(
        IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await _cache.GetStringAsync(key);

        if (data == null)
            return default;

        return JsonSerializer.Deserialize<T>(data);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration)
    {
        var options =
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    expiration
            };

        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            options);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}