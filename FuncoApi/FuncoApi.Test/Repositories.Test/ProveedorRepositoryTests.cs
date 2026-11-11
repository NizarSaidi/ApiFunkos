using FuncoApi.Context.Mongo;
using FuncoApi.Entity;
using FuncoApi.Repositories;
using FuncoApi.Repositories.proveedoresRepository;
using Moq;

namespace FuncoApi.Test.Repositories.Test;

/// <summary>Comprueba que el repositorio de proveedores delega las operaciones en su fuente de datos.</summary>
public class ProveedorRepositoryTests
{
    private readonly Mock<IProveedorDataSource> _dataSource = new();
    private readonly ProveedorRepository _repository;

    /// <summary>Inicializa el repositorio con una fuente de datos simulada.</summary>
    public ProveedorRepositoryTests()
    {
        _repository = new ProveedorRepository(_dataSource.Object);
    }

    /// <summary>Comprueba que el listado devuelve todos los proveedores de la fuente de datos.</summary>
    [Fact]
    public async Task GetAllAsync_Returns_All_Proveedores()
    {
        var proveedores = new List<ProveedorEntity>
        {
            new() { Id = 1, Nombre = "Proveedor A" },
            new() { Id = 2, Nombre = "Proveedor B" }
        };

        _dataSource.Setup(ds => ds.GetAllAsync()).ReturnsAsync(proveedores);

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Id == 1 && p.Nombre == "Proveedor A");
        Assert.Contains(result, p => p.Id == 2 && p.Nombre == "Proveedor B");
        _dataSource.Verify(ds => ds.GetAllAsync(), Times.Once);
    }

    /// <summary>Comprueba que la búsqueda devuelve el proveedor existente.</summary>
    [Fact]
    public async Task GetByIdAsync_Returns_Proveedor_When_Exists()
    {
        var proveedor = new ProveedorEntity { Id = 5, Nombre = "Proveedor X" };
        _dataSource.Setup(ds => ds.GetByIdAsync(5)).ReturnsAsync(proveedor);

        var result = await _repository.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Proveedor X", result.Nombre);
        _dataSource.Verify(ds => ds.GetByIdAsync(5), Times.Once);
    }

    /// <summary>Comprueba que la búsqueda propaga el resultado nulo de la fuente de datos.</summary>
    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        _dataSource.Setup(ds => ds.GetByIdAsync(999)).ReturnsAsync((ProveedorEntity?)null);

        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
        _dataSource.Verify(ds => ds.GetByIdAsync(999), Times.Once);
    }

    /// <summary>Comprueba que la creación delega la inserción y devuelve el proveedor creado.</summary>
    [Fact]
    public async Task CreateAsync_Inserts_Proveedor()
    {
        var proveedor = new ProveedorEntity { Id = 7, Nombre = "Nuevo" };
        _dataSource.Setup(ds => ds.CreateAsync(proveedor)).ReturnsAsync(proveedor);

        var result = await _repository.CreateAsync(proveedor);

        Assert.NotNull(result);
        Assert.Equal(7, result.Id);
        Assert.Equal("Nuevo", result.Nombre);
        _dataSource.Verify(ds => ds.CreateAsync(proveedor), Times.Once);
    }

    /// <summary>Comprueba que la eliminación devuelve el proveedor cuando existe.</summary>
    [Fact]
    public async Task DeleteAsync_Returns_Proveedor_When_Exists()
    {
        var proveedor = new ProveedorEntity { Id = 3, Nombre = "A borrar" };
        _dataSource.Setup(ds => ds.DeleteAsync(3)).ReturnsAsync(proveedor);

        var result = await _repository.DeleteAsync(3);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("A borrar", result.Nombre);
        _dataSource.Verify(ds => ds.DeleteAsync(3), Times.Once);
    }

    /// <summary>Comprueba que la eliminación propaga un resultado nulo cuando no existe.</summary>
    [Fact]
    public async Task DeleteAsync_Returns_Null_When_Not_Found()
    {
        _dataSource.Setup(ds => ds.DeleteAsync(123)).ReturnsAsync((ProveedorEntity?)null);

        var result = await _repository.DeleteAsync(123);

        Assert.Null(result);
        _dataSource.Verify(ds => ds.DeleteAsync(123), Times.Once);
    }

    /// <summary>Comprueba que la actualización delega y devuelve el proveedor actualizado.</summary>
    [Fact]
    public async Task UpdateAsync_Updates_Existing_Proveedor()
    {
        var updated = new ProveedorEntity { Id = 9, Nombre = "Nuevo" };
        _dataSource.Setup(ds => ds.UpdateAsync(updated)).ReturnsAsync(updated);

        var result = await _repository.UpdateAsync(updated);

        Assert.NotNull(result);
        Assert.Equal(9, result.Id);
        Assert.Equal("Nuevo", result.Nombre);
        _dataSource.Verify(ds => ds.UpdateAsync(updated), Times.Once);
    }

    /// <summary>Comprueba que la actualización propaga un resultado nulo cuando no existe.</summary>
    [Fact]
    public async Task UpdateAsync_Returns_Null_When_Not_Found()
    {
        var proveedor = new ProveedorEntity { Id = 321, Nombre = "No existe" };
        _dataSource.Setup(ds => ds.UpdateAsync(proveedor)).ReturnsAsync((ProveedorEntity?)null);

        var result = await _repository.UpdateAsync(proveedor);

        Assert.Null(result);
        _dataSource.Verify(ds => ds.UpdateAsync(proveedor), Times.Once);
    }
}
