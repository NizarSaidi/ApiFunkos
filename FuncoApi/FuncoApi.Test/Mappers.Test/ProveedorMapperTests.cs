using FuncoApi.Entity;
using FuncoApi.Mappers;
using FuncoApi.Models;

namespace FuncoApi.Test.Mappers.Test;

public class ProveedorMapperTests
{
    [Fact]
    public void ToModel_CopiesProviderFields()
    {
        var entity = new ProveedorEntity { Id = 8, Nombre = "Distribuidor" };

        var model = ProveedorMapper.ToModel(entity);

        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Nombre, model.Nombre);
    }

    [Fact]
    public void ToEntity_CopiesProviderFields()
    {
        var model = new Proveedor { Id = 8, Nombre = "Distribuidor" };

        var entity = ProveedorMapper.ToEntity(model);

        Assert.Equal(model.Id, entity.Id);
        Assert.Equal(model.Nombre, entity.Nombre);
    }
}
