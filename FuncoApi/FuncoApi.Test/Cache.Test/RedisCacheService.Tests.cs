using System.Text;
using System.Text.Json;
using FuncoApi.Cache;
using Microsoft.Extensions.Caching.Distributed;

namespace FuncoApi.Test.Cache.Test;

/// <summary>Comprueba serialización y operaciones del adaptador de caché distribuida.</summary>
public class RedisCacheServiceTests
{
    /// <summary>Comprueba que una clave ausente produce un valor nulo.</summary>
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        var cache = new FakeDistributedCache();
        var service = new RedisCacheService(cache);

        var result = await service.GetAsync<TestItem>("missing-key");

        Assert.Null(result);
    }

    /// <summary>Comprueba que guardar un valor produce JSON en la caché subyacente.</summary>
    [Fact]
    public async Task SetAsync_SerializesAndStoresValue()
    {
        var cache = new FakeDistributedCache();
        var service = new RedisCacheService(cache);
        var value = new TestItem { Name = "Funko" };
        var expiration = TimeSpan.FromMinutes(5);

        await service.SetAsync("funko", value, expiration);

        Assert.True(cache.ContainsKey("funko"));
        var storedJson = Encoding.UTF8.GetString(cache.Get("funko")!);
        Assert.Contains("Funko", storedJson);
        Assert.Contains(nameof(TestItem.Name), storedJson);
    }

    /// <summary>Comprueba que eliminar una clave la quita de la caché subyacente.</summary>
    [Fact]
    public async Task RemoveAsync_DeletesKey()
    {
        var cache = new FakeDistributedCache();
        var service = new RedisCacheService(cache);
        cache.Set("funko", Encoding.UTF8.GetBytes("{\"Name\":\"Funko\"}"), new DistributedCacheEntryOptions());

        await service.RemoveAsync("funko");

        Assert.False(cache.ContainsKey("funko"));
    }

    /// <summary>Comprueba que el JSON almacenado se deserializa al tipo solicitado.</summary>
    [Fact]
    public async Task GetAsync_DeserializesStoredJson()
    {
        var cache = new FakeDistributedCache();
        var value = new TestItem { Name = "Funko" };
        var serialized = JsonSerializer.Serialize(value);
        cache.Set("funko", Encoding.UTF8.GetBytes(serialized), new DistributedCacheEntryOptions());
        var service = new RedisCacheService(cache);

        var result = await service.GetAsync<TestItem>("funko");

        Assert.NotNull(result);
        Assert.Equal("Funko", result.Name);
    }

    /// <summary>Valor de prueba para la conversión JSON de objetos almacenados.</summary>
    private sealed class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Implementación en memoria de <see cref="IDistributedCache"/> usada por las pruebas.</summary>
    private sealed class FakeDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _values = new();

        public byte[]? Get(string key)
        {
            return _values.TryGetValue(key, out var value) ? value : null;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _values[key] = value;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _values.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }
    }
}