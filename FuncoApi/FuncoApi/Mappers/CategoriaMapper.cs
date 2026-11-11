using FuncoApi.Entity;
using FuncoApi.Models;

namespace FuncoApi.Mappers;

/// <summary>Mapea categorías entre los modelos de aplicación y las entidades persistidas.</summary>
public static class CategoriaMapper
{
    /// <summary>Convierte una entidad persistida en el modelo de aplicación.</summary>
    public static Categoria ToModel(CategoriaEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre
    };

    /// <summary>Convierte un modelo en la entidad persistible.</summary>
    public static CategoriaEntity ToEntity(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre
    };
}
