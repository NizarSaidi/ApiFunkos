using FuncoApi.Cache;
using FuncoApi.Context;
using FuncoApi.Context.Mongo;
using FuncoApi.Domain;
using FuncoApi.Entity;
using FuncoApi.Models;
using FuncoApi.Repositories;
using FuncoApi.Repositories.proveedoresRepository;
using FuncoApi.Services.CategoriaService;
using FuncoApi.Services.FunkosService;
using FuncoApi.Services.ProveedoresService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Mongo2Go;

namespace FuncoApi.Test.Services.IntegrationTest;

/// <summary>Pruebas de integración de servicios con repositorios y caché reales.</summary>
public class ServiceIntegrationTests : IAsyncLifetime
{
    private MongoDbRunner _mongoRunner = null!;

    /// <summary>Inicia MongoDB temporal para el servicio de proveedores.</summary>
    public Task InitializeAsync()
    {
        _mongoRunner = MongoDbRunner.Start(singleNodeReplSet: false);
        return Task.CompletedTask;
    }

    /// <summary>Detiene MongoDB temporal tras las pruebas.</summary>
    public Task DisposeAsync()
    {
        _mongoRunner.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task FunkoService_CompletesCrudAndRefreshesCache()
    {
        await using var context = CreateSqlContext();
        var cache = CreateCache();
        var service = new FunkoService(cache, new FunkoRepository(context));

        var created = await service.Create(CreateValidFunko());

        Assert.True(created.IsSuccess);
        var id = created.Value.Id;
        Assert.True(id > 0);

        var sameNameUpdate = CreateValidFunko();
        sameNameUpdate.Id = id;
        Assert.True((await service.Update(id, sameNameUpdate)).IsSuccess);
        var duplicateCreate = await service.Create(CreateValidFunko());
        Assert.Equal(DomainErrorCode.Conflict, duplicateCreate.Error.Code);
        var otherFunko = CreateValidFunko();
        otherFunko.Nombre = "Iron Man";
        Assert.True((await service.Create(otherFunko)).IsSuccess);

        var listed = await service.GetAll();
        Assert.True(listed.IsSuccess);
        Assert.Contains(listed.Value, funko => funko.Id == id && funko.Nombre == "Spider-Man");

        var found = await service.GetById(id);
        Assert.True(found.IsSuccess);
        Assert.Equal("Spider-Man", found.Value.Nombre);
        var cached = await service.GetById(id);
        Assert.True(cached.IsSuccess);

        var updated = CreateValidFunko();
        updated.Id = id;
        updated.Nombre = "Iron Man";
        var duplicateUpdate = await service.Update(id, updated);
        Assert.Equal(DomainErrorCode.Conflict, duplicateUpdate.Error.Code);

        updated.Nombre = "Spider-Man actualizado";
        var updateResult = await service.Update(id, updated);
        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Spider-Man actualizado", updateResult.Value.Nombre);
        Assert.Equal("Spider-Man actualizado", (await service.GetById(id)).Value.Nombre);

        var deleted = await service.Delete(id);
        Assert.True(deleted.IsSuccess);
        Assert.Equal(id, deleted.Value.Id);
        var missing = await service.GetById(id);
        Assert.True(missing.IsFailure);
        Assert.Equal(DomainErrorCode.NotFound, missing.Error.Code);
    }

    [Fact]
    public async Task CategoriaService_CompletesCrudAndRefreshesCache()
    {
        await using var context = CreateSqlContext();
        var cache = CreateCache();
        var service = new CategoriaService(cache, new CategoriaRepository(context));
        var category = new Categoria { Id = "anime", Nombre = "Anime" };

        var emptyList = await service.GetAll();
        Assert.Empty(emptyList.Value);
        Assert.Same(emptyList.Value, (await service.GetAll()).Value);
        var created = await service.Create(category);

        Assert.True(created.IsSuccess);
        Assert.Equal(category.Id, created.Value.Id);
        Assert.True((await service.Update(category.Id,
            new Categoria { Id = category.Id, Nombre = category.Nombre })).IsSuccess);
        Assert.Equal(DomainErrorCode.Conflict,
            (await service.Create(new Categoria { Id = "manga", Nombre = "Anime" })).Error.Code);
        Assert.True((await service.Create(new Categoria { Id = "manga", Nombre = "Manga" })).IsSuccess);
        var listed = await service.GetAll();
        Assert.True(listed.IsSuccess);
        Assert.Contains(listed.Value, item => item.Id == category.Id);

        var found = await service.GetById(category.Id);
        Assert.True(found.IsSuccess);
        Assert.Equal(category.Nombre, found.Value.Nombre);
        Assert.Same(found.Value, (await service.GetById(category.Id)).Value);

        var updated = new Categoria { Id = category.Id, Nombre = "Animación" };
        Assert.Equal(DomainErrorCode.Conflict, (await service.Update(
            category.Id, new Categoria { Id = category.Id, Nombre = "Manga" })).Error.Code);
        var updateResult = await service.Update(category.Id, updated);
        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Animación", (await service.GetById(category.Id)).Value.Nombre);
        Assert.Contains((await service.GetAll()).Value, item => item.Nombre == "Animación");

        var deleted = await service.Delete(category.Id);
        Assert.True(deleted.IsSuccess);
        Assert.True((await service.Delete("manga")).IsSuccess);
        var emptyAfterDelete = await service.GetAll();
        Assert.Empty(emptyAfterDelete.Value);
        Assert.Same(emptyAfterDelete.Value, (await service.GetAll()).Value);
        var missing = await service.GetById(category.Id);
        Assert.True(missing.IsFailure);
        Assert.Equal(DomainErrorCode.NotFound, missing.Error.Code);
        Assert.Equal(DomainErrorCode.NotFound, (await service.Update(
            category.Id, new Categoria { Id = category.Id, Nombre = "Anime" })).Error.Code);
        Assert.Equal(DomainErrorCode.NotFound, (await service.Delete(category.Id)).Error.Code);
    }

    [Fact]
    public async Task ProveedorService_CompletesCrudAndRefreshesCache()
    {
        var databaseName = $"service_integration_{Guid.NewGuid():N}";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoSettings:ConnectionString"] = _mongoRunner.ConnectionString,
                ["MongoSettings:DatabaseName"] = databaseName
            })
            .Build();
        var context = new MongoDbContext(configuration);
        var service = new ProveedorService(
            CreateCache(),
            new ProveedorRepository(new MongoProveedorDataSource(context)));
        var provider = new Proveedor { Id = 1, Nombre = "Distribuidor" };

        var emptyList = await service.GetAll();
        Assert.Empty(emptyList.Value);
        Assert.Same(emptyList.Value, (await service.GetAll()).Value);
        var created = await service.Create(provider);

        Assert.True(created.IsSuccess);
        Assert.Equal(provider.Id, created.Value.Id);
        Assert.True((await service.Update(provider.Id,
            new Proveedor { Id = provider.Id, Nombre = provider.Nombre })).IsSuccess);
        Assert.Equal(DomainErrorCode.Conflict, (await service.Create(
            new Proveedor { Id = 2, Nombre = "Distribuidor" })).Error.Code);
        Assert.True((await service.Create(new Proveedor { Id = 2, Nombre = "Mayorista" })).IsSuccess);
        var listed = await service.GetAll();
        Assert.True(listed.IsSuccess);
        Assert.Contains(listed.Value, item => item.Id == provider.Id);

        var found = await service.GetById(provider.Id);
        Assert.True(found.IsSuccess);
        Assert.Equal(provider.Nombre, found.Value.Nombre);
        Assert.Same(found.Value, (await service.GetById(provider.Id)).Value);

        Assert.Equal(DomainErrorCode.Conflict, (await service.Update(provider.Id,
            new Proveedor { Id = provider.Id, Nombre = "Mayorista" })).Error.Code);
        var updated = new Proveedor { Id = provider.Id, Nombre = "Distribuidor actualizado" };
        var updateResult = await service.Update(provider.Id, updated);
        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Distribuidor actualizado", (await service.GetById(provider.Id)).Value.Nombre);
        Assert.Contains((await service.GetAll()).Value, item => item.Nombre == "Distribuidor actualizado");

        var deleted = await service.Delete(provider.Id);
        Assert.True(deleted.IsSuccess);
        Assert.True((await service.Delete(2)).IsSuccess);
        var emptyAfterDelete = await service.GetAll();
        Assert.Empty(emptyAfterDelete.Value);
        Assert.Same(emptyAfterDelete.Value, (await service.GetAll()).Value);
        var missing = await service.GetById(provider.Id);
        Assert.True(missing.IsFailure);
        Assert.Equal(DomainErrorCode.NotFound, missing.Error.Code);
        Assert.Equal(DomainErrorCode.NotFound, (await service.Update(
            provider.Id, new Proveedor { Id = provider.Id, Nombre = "Distribuidor" })).Error.Code);
        Assert.Equal(DomainErrorCode.NotFound, (await service.Delete(provider.Id)).Error.Code);
    }

    private static FuncoDbContext CreateSqlContext()
    {
        var options = new DbContextOptionsBuilder<FuncoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FuncoDbContext(options);
    }

    private static MemoryCacheService CreateCache() =>
        new(new MemoryCache(new MemoryCacheOptions()));

    private static Funko CreateValidFunko() => new()
    {
        Nombre = "Spider-Man",
        Imagen = "spiderman.png",
        Precio = 14.99m,
        Stock = 5,
        Categoria = "marvel",
        Proveedor = 1
    };
}
