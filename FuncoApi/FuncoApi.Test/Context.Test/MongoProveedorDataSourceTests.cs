using FuncoApi.Context.Mongo;
using FuncoApi.Entity;
using Microsoft.Extensions.Configuration;
using Mongo2Go;

namespace FuncoApi.Test.Context.Test;

/// <summary>Pruebas de integración del CRUD de proveedores contra una instancia MongoDB temporal.</summary>
public class MongoProveedorDataSourceTests : IAsyncLifetime
{
    private MongoDbRunner _runner = null!;
    private MongoDbContext _context = null!;
    private MongoProveedorDataSource _dataSource = null!;

    /// <summary>Inicia Mongo2Go, configura el contexto y deja vacía la colección de prueba.</summary>
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

    /// <summary>Detiene la instancia MongoDB temporal al finalizar las pruebas.</summary>
    public Task DisposeAsync()
    {
        _runner.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>Comprueba que la consulta devuelve todos los proveedores almacenados.</summary>
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

    /// <summary>Comprueba que la consulta por ID devuelve el proveedor existente.</summary>
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

    /// <summary>Comprueba que la consulta por un ID desconocido devuelve nulo.</summary>
    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        var result = await _dataSource.GetByIdAsync(999);

        Assert.Null(result);
    }

    /// <summary>Comprueba que crear un proveedor lo inserta y lo devuelve.</summary>
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

    /// <summary>Comprueba que borrar un proveedor devuelve el documento y lo elimina de MongoDB.</summary>
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

    /// <summary>Comprueba que borrar un proveedor inexistente devuelve nulo.</summary>
    [Fact]
    public async Task DeleteAsync_Returns_Null_When_Not_Found()
    {
        var result = await _dataSource.DeleteAsync(123);

        Assert.Null(result);
    }

    /// <summary>Comprueba que actualizar un proveedor existente reemplaza sus datos.</summary>
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

    /// <summary>Comprueba que actualizar un proveedor inexistente devuelve nulo.</summary>
    [Fact]
    public async Task UpdateAsync_Returns_Null_When_Not_Found()
    {
        var result = await _dataSource.UpdateAsync(new ProveedorEntity { Id = 321, Nombre = "No existe" });

        Assert.Null(result);
    }
}
