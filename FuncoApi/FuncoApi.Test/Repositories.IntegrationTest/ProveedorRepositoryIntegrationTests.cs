using FuncoApi.Context.Mongo;
using FuncoApi.Entity;
using FuncoApi.Repositories.proveedoresRepository;
using Microsoft.Extensions.Configuration;
using Mongo2Go;

namespace FuncoApi.Test.Repositories.IntegrationTest;

/// <summary>Prueba el repositorio de proveedores contra MongoDB real temporal.</summary>
public class ProveedorRepositoryIntegrationTests : IAsyncLifetime
{
    private MongoDbRunner _runner = null!;
    private ProveedorRepository _repository = null!;

    public Task InitializeAsync()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: false);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoSettings:ConnectionString"] = _runner.ConnectionString,
                ["MongoSettings:DatabaseName"] = $"repository_{Guid.NewGuid():N}"
            })
            .Build();
        _repository = new ProveedorRepository(
            new MongoProveedorDataSource(new MongoDbContext(configuration)));
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _runner.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Repository_CreatesPaginatesUpdatesAndDeletesProviders()
    {
        var first = await _repository.CreateAsync(new ProveedorEntity { Nombre = "Distribuidor" });
        var second = await _repository.CreateAsync(new ProveedorEntity { Nombre = "Mayorista" });
        await _repository.CreateAsync(new ProveedorEntity { Nombre = "Tienda" });

        Assert.NotNull(first);
        Assert.True(first.Id > 0);
        Assert.True(await _repository.ExistsByNameAsync("Distribuidor"));
        Assert.False(await _repository.ExistsByNameAsync("Distribuidor", first.Id));
        Assert.Equal("Distribuidor", (await _repository.GetByIdAsync(first.Id))!.Nombre);

        var page = await _repository.GetAllAsync(page: 2, pageSize: 1);
        Assert.True(page.IsSuccess);
        Assert.Equal(3, page.Value.Count);
        Assert.Equal("Mayorista", Assert.Single(page.Value.Objs).Nombre);

        first.Nombre = "Distribuidor actualizado";
        var updated = await _repository.UpdateAsync(first);
        Assert.NotNull(updated);
        Assert.Equal("Distribuidor actualizado", (await _repository.GetByIdAsync(first.Id))!.Nombre);
        Assert.False(await _repository.ExistsByNameAsync("Distribuidor"));

        var deleted = await _repository.DeleteAsync(second!.Id);
        Assert.NotNull(deleted);
        Assert.Equal("Mayorista", deleted.Nombre);
        Assert.Null(await _repository.GetByIdAsync(second.Id));
        var afterDelete = await _repository.GetAllAsync(1, 10);
        Assert.Equal(2, afterDelete.Value.Count);
        Assert.DoesNotContain(afterDelete.Value.Objs, provider => provider.Id == second.Id);
    }
}
