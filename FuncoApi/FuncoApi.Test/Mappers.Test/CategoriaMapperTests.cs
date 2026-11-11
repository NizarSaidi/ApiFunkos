using FuncoApi.Entity;
using FuncoApi.Mappers;
using FuncoApi.Models;

namespace FuncoApi.Test.Mappers.Test;

public class CategoriaMapperTests
{
    [Fact]
    public void ToModel_CopiesCategoryFields()
    {
        var entity = new CategoriaEntity { Id = "anime", Nombre = "Anime" };

        var model = CategoriaMapper.ToModel(entity);

        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Nombre, model.Nombre);
    }

    [Fact]
    public void ToEntity_CopiesCategoryFields()
    {
        var model = new Categoria { Id = "anime", Nombre = "Anime" };

        var entity = CategoriaMapper.ToEntity(model);

        Assert.Equal(model.Id, entity.Id);
        Assert.Equal(model.Nombre, entity.Nombre);
    }
}
