using FuncoApi.Context.Mongo;
using FuncoApi.Entity;
using Microsoft.Extensions.Configuration;
using Mongo2Go;

namespace FuncoApi.Test.Context.Test;

public class MongoProveedorDataSourceTests : IAsyncLifetime
{
    private MongoDbRunner _runner = null!;
    private MongoDbContext _context = null!;
    private MongoProveedorDataSource _dataSource = null!;

    public async Task InitializeAsync()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: false);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoSettings:ConnectionString"] = _runner.ConnectionString,
                ["MongoSettings:DatabaseName"] = "test_proveedor_datasource_db"
            })
            .Build();

        _context = new MongoDbContext(config);
        _dataSource = new MongoProveedorDataSource(_context);

        await _context.Proveedores.Database.DropCollectionAsync("proveedores");
    }

    public Task DisposeAsync()
    {
        _runner.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllAsync_Returns_All_Proveedores()
    {
        await _context.Proveedores.InsertManyAsync(new[]
        {
            new ProveedorEntity { Id = 1, Nombre = "Proveedor A" },
            new ProveedorEntity { Id = 2, Nombre = "Proveedor B" }
        });

        var result = await _dataSource.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Id == 1 && p.Nombre == "Proveedor A");
        Assert.Contains(result, p => p.Id == 2 && p.Nombre == "Proveedor B");
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Proveedor_When_Exists()
    {
        var proveedor = new ProveedorEntity { Id = 5, Nombre = "Proveedor X" };
        await _context.Proveedores.InsertOneAsync(proveedor);

        var result = await _dataSource.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
        Assert.Equal("Proveedor X", result.Nombre);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        var result = await _dataSource.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_Inserts_Proveedor_And_Returns_It()
    {
        var proveedor = new ProveedorEntity { Id = 7, Nombre = "Nuevo" };

        var result = await _dataSource.CreateAsync(proveedor);

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        Assert.Equal("Nuevo", result.Nombre);
        Assert.NotNull(await _dataSource.GetByIdAsync(7));
    }

    [Fact]
    public async Task DeleteAsync_Returns_Proveedor_When_Exists()
    {
        await _context.Proveedores.InsertOneAsync(new ProveedorEntity { Id = 3, Nombre = "A borrar" });

        var result = await _dataSource.DeleteAsync(3);

        Assert.NotNull(result);
        Assert.Equal(3, result!.Id);
        Assert.Equal("A borrar", result.Nombre);
        Assert.Null(await _dataSource.GetByIdAsync(3));
    }

    [Fact]
    public async Task DeleteAsync_Returns_Null_When_Not_Found()
    {
        var result = await _dataSource.DeleteAsync(123);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Updates_Existing_Proveedor()
    {
        await _context.Proveedores.InsertOneAsync(new ProveedorEntity { Id = 9, Nombre = "Viejo" });

        var result = await _dataSource.UpdateAsync(new ProveedorEntity { Id = 9, Nombre = "Nuevo" });

        Assert.NotNull(result);
        Assert.Equal(9, result!.Id);
        Assert.Equal("Nuevo", result.Nombre);
        var stored = await _dataSource.GetByIdAsync(9);
        Assert.NotNull(stored);
        Assert.Equal("Nuevo", stored!.Nombre);
    }

    [Fact]
    public async Task UpdateAsync_Returns_Null_When_Not_Found()
    {
        var result = await _dataSource.UpdateAsync(new ProveedorEntity { Id = 321, Nombre = "No existe" });

        Assert.Null(result);
    }
}
