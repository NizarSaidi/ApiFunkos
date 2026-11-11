using FuncoApi.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace FuncoApi.Test.Cache.Test;

public class MemoryCacheServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheService(cache);

        var result = await service.GetAsync<string>("missing-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_AndGetAsync_ReturnStoredValue()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheService(cache);
        var value = new TestItem { Name = "Funko" };

        await service.SetAsync("funko", value, TimeSpan.FromMinutes(5));
        var result = await service.GetAsync<TestItem>("funko");

        Assert.NotNull(result);
        Assert.Equal("Funko", result.Name);
    }

    [Fact]
    public async Task RemoveAsync_RemovesStoredValue()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheService(cache);
        var value = new TestItem { Name = "Funko" };

        await service.SetAsync("funko", value, TimeSpan.FromMinutes(5));
        await service.RemoveAsync("funko");

        var result = await service.GetAsync<TestItem>("funko");
        Assert.Null(result);
    }

    private sealed class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }
}
