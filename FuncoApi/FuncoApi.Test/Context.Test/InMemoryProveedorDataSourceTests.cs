using FuncoApi.Context.Mongo;
using FuncoApi.Entity;

namespace FuncoApi.Test.Context.Test;

/// <summary>Pruebas del almacenamiento en memoria usado por proveedores en desarrollo.</summary>
public class InMemoryProveedorDataSourceTests
{
    [Fact]
    public async Task CreateAndRead_ReturnsAssignedIdAndStoredProvider()
    {
        var source = new InMemoryProveedorDataSource();

        var created = await source.CreateAsync(new ProveedorEntity { Nombre = "Proveedor temporal" });
        var loaded = await source.GetByIdAsync(created!.Id);

        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded.Id);
        Assert.Equal("Proveedor temporal", loaded.Nombre);
    }

    [Fact]
    public async Task GetAll_ReturnsRequestedPageAndTotalCount()
    {
        var source = new InMemoryProveedorDataSource();
        await source.CreateAsync(new ProveedorEntity { Nombre = "Proveedor A" });
        await source.CreateAsync(new ProveedorEntity { Nombre = "Proveedor B" });

        var result = await source.GetAllAsync(page: 2, pageSize: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Proveedor B", Assert.Single(result.Value.Objs).Nombre);
    }

    [Fact]
    public async Task UpdateAndDelete_ChangesStoredProvider()
    {
        var source = new InMemoryProveedorDataSource();
        var created = await source.CreateAsync(new ProveedorEntity { Nombre = "Nombre original" });

        var updated = await source.UpdateAsync(new ProveedorEntity
        {
            Id = created!.Id,
            Nombre = "Nombre actualizado"
        });
        var deleted = await source.DeleteAsync(created.Id);

        Assert.Equal("Nombre actualizado", updated!.Nombre);
        Assert.Equal("Nombre actualizado", deleted!.Nombre);
        Assert.Null(await source.GetByIdAsync(created.Id));
    }
}
