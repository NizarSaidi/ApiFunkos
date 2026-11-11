using FuncoApi.Cache.objet;
using Microsoft.Extensions.Caching.Memory;

namespace FuncoApi.Cache;

/// <summary>Implementa <see cref="ICacheService"/> usando la caché local en memoria.</summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    /// <summary>Crea el adaptador para una caché de memoria de ASP.NET Core.</summary>
    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }
    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);

        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        _cache.Set(key, value, expiration);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);

        return Task.CompletedTask;
    }
}