using FuncoApi.Entity;
using FuncoApi.Mappers;
using FuncoApi.Models;

namespace FuncoApi.Test.Mappers.Test;

public class FunkoMapperTests
{
    [Fact]
    public void ToModel_CopiesAllApiFields()
    {
        var entity = new FunkoEntity
        {
            Id = 12,
            Nombre = "Spider-Man",
            Imagen = "spiderman.png",
            Precio = 14.99m,
            Stock = 5,
            Categoria = "marvel",
            Proveedor = 2
        };

        var model = FunkoMapper.ToModel(entity);

        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Nombre, model.Nombre);
        Assert.Equal(entity.Imagen, model.Imagen);
        Assert.Equal(entity.Precio, model.Precio);
        Assert.Equal(entity.Stock, model.Stock);
        Assert.Equal(entity.Categoria, model.Categoria);
        Assert.Equal(entity.Proveedor, model.Proveedor);
    }

    [Fact]
    public void ToEntity_CopiesAllPersistedApiFields()
    {
        var model = new Funko
        {
            Id = 12,
            Nombre = "Spider-Man",
            Imagen = "spiderman.png",
            Precio = 14.99m,
            Stock = 5,
            Categoria = "marvel",
            Proveedor = 2
        };

        var entity = FunkoMapper.ToEntity(model);

        Assert.Equal(model.Id, entity.Id);
        Assert.Equal(model.Nombre, entity.Nombre);
        Assert.Equal(model.Imagen, entity.Imagen);
        Assert.Equal(model.Precio, entity.Precio);
        Assert.Equal(model.Stock, entity.Stock);
        Assert.Equal(model.Categoria, entity.Categoria);
        Assert.Equal(model.Proveedor, entity.Proveedor);
    }
}
